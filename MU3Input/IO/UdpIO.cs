using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MU3Input
{
    public class UdpIO : IO
    {
        const int defaultPort = 4354;
        uint currentLedData = 0;
        UdpClient client;
        IPEndPoint savedEP;
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        public UdpIO()
        {
            _data = new OutputData() { Buttons = new byte[10], AimiId = new byte[10] };
            client = new UdpClient(GetPort());
            new Thread(PollThread).Start();
        }
        public int GetPort()
        {
            var location = GetType().Assembly.Location;
            string directoryName = Path.GetDirectoryName(location);
            string segatoolsIniPath = Path.Combine(directoryName, "segatools.ini");
            if (File.Exists(segatoolsIniPath))
            {
                StringBuilder temp = new StringBuilder();
                TcpIO.GetPrivateProfileString("mu3io", "port", defaultPort.ToString(), temp, 1024, segatoolsIniPath);
                if (int.TryParse(temp.ToString(), out int port))
                {
                    return port;
                }
            }
            return defaultPort;
        }
        // 不需要长连接
        public override bool IsConnected => true;
        public override void Reconnect() { }

        private void PollThread()
        {
            while (true)
            {
                byte[] buffer = client?.Receive(ref remoteEP);
                // 如果是一个新连接则记录，并发送颜色数据
                if (remoteEP != savedEP)
                {
                    savedEP = new IPEndPoint(remoteEP.Address, remoteEP.Port);
                }
                ParseBuffer(buffer);
            }
        }

        private void ParseBuffer(byte[] buffer)
        {
            if ((buffer?.Length ?? 0) == 0) return;
            if (buffer[0] == (byte)MessageType.ButtonStatus && buffer.Length == 3)
            {
                int index = buffer[1];
                _data.Buttons[index] = buffer[2];
            }
            else if (buffer[0] == (byte)MessageType.MoveLever && buffer.Length == 3)
            {
                var value = (short)(buffer[2] << 8 | buffer[1]);
                _data.Lever = value;
            }
            else if (buffer[0] == (byte)MessageType.Scan && buffer.Length == 12)
            {
                _data.Scan = buffer[1] > 0;
                _data.AimiId = new ArraySegment<byte>(buffer, 2, 10).ToArray();
            }
            else if (buffer[0] == (byte)MessageType.Test && buffer.Length == 2)
            {
                _data.OptButton = buffer[1];
            }
            else if (buffer[0] == (byte)MessageType.RequestColors && buffer.Length == 1)
            {
                SetLed(currentLedData);
            }
            // 收到心跳数据直接回传原数据表示在线
            else if (buffer[0] == (byte)MessageType.DokiDoki && buffer.Length == 2)
            {
                client.SendAsync(buffer, 2, savedEP);
            }
        }

        public override unsafe void SetLed(uint data)
        {
            currentLedData = data;
            if (savedEP != null)
            {
                client?.SendAsync(new byte[] { (byte)MessageType.SetLed }.Concat(BitConverter.GetBytes(data)).ToArray(), 5, savedEP);
            }
        }
        public override unsafe void SetAimiId(byte[] id)
        {
            // 正常游戏无需实现也不会使用
            return;
        }
        enum MessageType : byte
        {
            // 控制器向IO发送的
            ButtonStatus = 1,
            MoveLever = 2,
            Scan = 3,
            Test = 4,
            RequestColors = 5,
            // IO向控制器发送的
            SetLed = 6,
            // 心跳检测连接状态(
            DokiDoki = 255
        }
        //VS谜之抽风在这个类导入GetPrivateProfileString无法编译
        //[DllImport("kernel32")]//返回取得字符串缓冲区的长度
        //private static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
    }
}
