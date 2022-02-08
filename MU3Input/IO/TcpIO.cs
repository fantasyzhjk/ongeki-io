using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MU3Input
{
    public class TcpIO : IO
    {
        const int defaultPort = 4354;
        uint currentLedData = 0;
        TcpListener listener;
        TcpClient client;
        NetworkStream networkStream;
        public TcpIO(int port)
        {
            _data = new OutputData() { Buttons = new byte[10], AimiId = new byte[10] };
            IPAddress ip = new IPAddress(new byte[] { 0, 0, 0, 0 });
            listener = new TcpListener(ip, port);
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
            SetLed(currentLedData);
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
        private byte[] _inBuffer = new byte[24];
        private void PollThread()
        {
            while (true)
            {
                if (!IsConnected)
                {
                    connectTask?.Wait();
                    continue;
                }
                IAsyncResult result = networkStream.BeginRead(_inBuffer, 0, _inBuffer.Length, new AsyncCallback((res) => { }), null);
                int len = networkStream.EndRead(result);
                if (len <= 0)
                {
                    Disconnect();
                    continue;
                }
                var temp = _inBuffer.ToStructure<OutputData>();
                if (temp.Scan == true && temp.AimiId.All(n => n == 255))
                {
                    temp.AimiId = Utils.ReadOrCreateAimeTxt();
                }
                _data = temp;
            }
        }
        public override unsafe void SetLed(uint data)
        {
            try
            {
                // 缓存led数据将其设置到新连接的设备
                currentLedData = data;
                if (!IsConnected)
                    return;
                networkStream.Write(BitConverter.GetBytes(data), 0, 4);
            }
            catch
            {
                return;
            }
        }
    }
}
