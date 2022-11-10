//using System;
//using System.Linq;
//using System.Net.Sockets;
//using System.Threading;

//namespace MU3Input
//{
//    public class TcpIO : IO
//    {
//        private string ip = "127.0.0.1";
//        private int port;
//        private uint currentLedData = 0;
//        private bool connecting = false;
//        private TcpClient client;
//        private NetworkStream networkStream;
//        protected OutputData data;

//        public TcpIO(int port)
//        {
//            this.port = port;
//            data = new OutputData() { Buttons = new byte[10], AimiId = new byte[10] };
//            new Thread(PollThread).Start();

//        }
//        public override bool IsConnected => client?.Connected ?? false;
//        public override OutputData Data => data;
//        // 重连
//        public override void Reconnect()
//        {
//            if (connecting) return;
//            Disconnect();
//            ConnectAsync(ip, port);
//            //connectTask?.Wait();
//        }
//        public void ConnectAsync(string ip, int port)
//        {
//            if (connecting) return;
//            connecting = true;
//            try
//            {
//                var newClient = new TcpClient(ip, port);
//                networkStream = newClient.GetStream();
//                client = newClient;
//                SetLed(currentLedData);
//            }
//            catch (Exception)
//            {
//                Disconnect();
//            }
//            connecting = false;
//        }
//        private void Disconnect()
//        {
//            if (IsConnected)
//            {
//                var tmpClient = client;
//                var tmpStream = networkStream;
//                client = null;
//                networkStream = null;
//                tmpClient?.Dispose();
//                tmpStream?.Dispose();
//            }
//        }
//        private byte[] _inBuffer = new byte[24];
//        private void PollThread()
//        {
//            while (true)
//            {
//                if (!IsConnected)
//                {
//                    Reconnect();
//                    continue;
//                }
//                int len = networkStream.Read(_inBuffer, 0, _inBuffer.Length);
//                if (len <= 0)
//                {
//                    Reconnect();
//                    continue;
//                }
//                var temp = _inBuffer.ToStructure<OutputData>();
//                if (temp.Scan == 1 && temp.AimiId.All(n => n == 255))
//                {
//                    temp.AimiId = Utils.ReadOrCreateAimeTxt();
//                }
//                data = temp;
//            }
//        }
//        public override unsafe void SetLed(uint data)
//        {
//            try
//            {
//                // 缓存led数据将其设置到新连接的设备
//                currentLedData = data;
//                if (!IsConnected)
//                    return;
//                networkStream.Write(BitConverter.GetBytes(data), 0, 4);
//            }
//            catch
//            {
//                return;
//            }
//        }
//    }
//}
