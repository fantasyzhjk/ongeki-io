using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MU3Input
{
    public class TcpIO : IO
    {
        const int defaultPort = 4354;
        TcpListener listener;
        TcpClient client;
        NetworkStream networkStream;
        public TcpIO()
        {
            _data = new OutputData() { Buttons = new byte[10], AimiId = new byte[10] };
            IPAddress ip = new IPAddress(new byte[] { 0, 0, 0, 0 });
            listener = new TcpListener(ip, GetPort());
            listener.Start();
            new Thread(PollThread).Start();
            //接收多个连接并只保留最后一个
            new Thread(async () =>
            {
                while (true)
                {
                    connectTask = ConnectAsync();
                    await connectTask;
                }
            }).Start();

        }
        public int GetPort()
        {
            var location = GetType().Assembly.Location;
            string directoryName = Path.GetDirectoryName(location);
            string segatoolsIniPath = Path.Combine(directoryName, "segatools.ini");
            if (File.Exists(segatoolsIniPath))
            {
                StringBuilder temp = new StringBuilder();
                GetPrivateProfileString("mu3io", "port", defaultPort.ToString(), temp, 1024, segatoolsIniPath);
                if(int.TryParse(temp.ToString(),out int port))
                {
                    return port;
                }
            }
            return defaultPort;
        }
        Task connectTask;
        public override bool IsConnected => client?.Connected ?? false;
        bool connecting = false;
        // 自动重连,无需外部调用
        public override void Reconnect()
        {
            //connectTask?.Wait();
        }
        public async Task ConnectAsync()
        {
            if (connecting) return;
            connecting = true;
            var newClient = await listener.AcceptTcpClientAsync();
            Disconnect();
            client = newClient;
            networkStream = client.GetStream();
            connecting = false;
        }
        private void Disconnect()
        {
            if (IsConnected)
            {
                networkStream?.Dispose();
                client?.Dispose();
                client = null;
                networkStream = null;
            }
        }
        private byte[] _inBuffer = new byte[23];
        private void PollThread()
        {
            while (true)
            {
                if (!IsConnected)
                {
                    connectTask?.Wait();
                    continue;
                }
                IAsyncResult result = networkStream.BeginRead(_inBuffer, 0, 23, new AsyncCallback((res) => { }), null);
                int len = networkStream.EndRead(result);
                if (len <= 0)
                {
                    Disconnect();
                    continue;
                }
                _data = _inBuffer.ToStructure<OutputData>();
                //// 用于直接打开测试显示按键
                //Mu3IO._test.UpdateData();
            }
        }

        public static int[] bitPosMap =
        {
            23, 19, 22, 20, 21, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6
        };

        public override unsafe void SetLed(uint data)
        {
            try
            {
                if (!IsConnected)
                    return;
                networkStream.Write(BitConverter.GetBytes(data), 0, 4);
            }
            catch
            {
                return;
            }
        }
        public override unsafe void SetAimiId(byte[] id)
        {
            if (!IsConnected)
                return;

            SetOptionInput input;
            input.Type = 1;

            fixed (void* src = id)
                CopyMemory(input.AimiId, src, 10);

            var outBuffer = new byte[64];
            fixed (void* d = outBuffer)
                CopyMemory(d, &input, 64);

            networkStream.Write(outBuffer, 0, outBuffer.Length);
        }
        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern unsafe void CopyMemory(void* dest, void* src, int count);
    }
}
