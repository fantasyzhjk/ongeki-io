using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace MU3Input
{
    public class TcpIO : IO
    {
        const int port = 4354;
        const int timeout = 1000;
        TcpListener listener;
        TcpClient client;
        NetworkStream networkStream;
        public TcpIO()
        {
            IPAddress ip = new IPAddress(new byte[] { 127, 0, 0, 1 });
            listener = new TcpListener(ip, port);
            listener.Start();
            Reconnect();
        }

        public override bool IsConnected => client?.Connected ?? false;

        public override async void Reconnect()
        {
            CloseClient();
            client = await listener.AcceptTcpClientAsync();
            networkStream = client.GetStream();
            new Thread(PollThread).Start();
        }
        private void CloseClient()
        {
            if (IsConnected)
            {
                networkStream?.Dispose();
                client?.Dispose();
                client = null;
                networkStream = null;
            }
        }
        private byte[] _inBuffer = new byte[64];
        private void PollThread()
        {
            while (true)
            {
                if (!IsConnected)
                    continue;
                IAsyncResult result = networkStream.BeginRead(_inBuffer, 0, 64, new AsyncCallback((res) => { }), null);
                int len = networkStream.EndRead(result);
                if (len <= 0)
                {
                    CloseClient();
                    return;
                }
                _data = _inBuffer.ToStructure<OutputData>();
                // 用于直接打开测试显示按键
                Mu3IO._test.UpdateData();
            }
        }

        public static int[] bitPosMap =
        {
            23, 19, 22, 20, 21, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6
        };

        public override unsafe void SetLed(uint data)
        {
            if (!IsConnected)
                return;

            SetLedInput led;
            led.Type = 0;
            led.LedBrightness = 40;

            for (var i = 0; i < 9; i++)
            {
                led.LedColors[i] = (byte)(((data >> bitPosMap[i]) & 1) * 255);
                led.LedColors[i + 15] = (byte)(((data >> bitPosMap[i + 9]) & 1) * 255);
            }

            var outBuffer = new byte[64];
            fixed (void* d = outBuffer)
                CopyMemory(d, &led, 64);

            networkStream.Write(outBuffer, 0, outBuffer.Length);
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
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern unsafe void CopyMemory(void* dest, void* src, int count);
    }
}
