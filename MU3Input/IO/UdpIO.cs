using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MU3Input
{
    public class UdpIO : IO
    {
        uint currentLedData = 0;
        UdpClient client;
        IPEndPoint savedEP;
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        public UdpIO(int port)
        {
            _data = new OutputData() { Buttons = new byte[10], AimiId = new byte[10] };
            client = new UdpClient(port);
            new Thread(PollThread).Start();
        }
        bool isConnected = false;
        public override bool IsConnected => isConnected;
        public override void Reconnect() { }

        private void PollThread()
        {
            while (true)
            {
                byte[] buffer = client?.Receive(ref remoteEP);
                // 如果已连接设备但收到了其他设备的消息则忽略
                if (IsConnected && (remoteEP.Address.Address != savedEP.Address.Address)) return;
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
                byte[] aimeId = new ArraySegment<byte>(buffer, 2, 10).ToArray();
                if (aimeId.All(n => n == 0))
                {
                    var location = this.GetType().Assembly.Location;
                    string directoryName = Path.GetDirectoryName(location);
                    string deviceDirectory = Path.Combine(directoryName, "DEVICE");
                    string aimeIdPath = Path.Combine(deviceDirectory, "aime.txt");
                    try
                    {
                        var id = BigInteger.Parse(File.ReadAllText(aimeIdPath));
                        var bytes = ToBcd(id);
                        aimeId = bytes.Concat(new byte[10 - bytes.Length]).ToArray();
                    }
                    catch (Exception ex)
                    {
                        Random random = new Random();
                        random.NextBytes(aimeId);
                        var id = new BigInteger(aimeId);
                        if (id < -1) id = -(id + 1);
                        id = id % BigInteger.Parse("99999999999999999999");
                        if (!Directory.Exists(deviceDirectory))
                        {
                            Directory.CreateDirectory(deviceDirectory);
                        }
                        File.WriteAllText(aimeIdPath, id.ToString());
                    }
                }
                _data.AimiId = aimeId;
            }
            else if (buffer[0] == (byte)MessageType.Test && buffer.Length == 2)
            {
                _data.OptButton = buffer[1];
            }
            else if (buffer[0] == (byte)MessageType.RequestValues && buffer.Length == 1)
            {
                SetLed(currentLedData);
                SetLever(_data.Lever);
            }
            // 收到心跳数据直接回传原数据表示在线，并保存其地址阻止其他设备连接
            else if (buffer[0] == (byte)MessageType.DokiDoki && buffer.Length == 2)
            {
                savedEP = new IPEndPoint(remoteEP.Address, remoteEP.Port);
                client.SendAsync(buffer, 2, savedEP);
                isConnected = true;
            }
        }
        public static byte[] ToBcd(BigInteger value)
        {
            var length = value.ToString().Length / 2 + value.ToString().Length % 2;
            byte[] ret = new byte[length];
            for (int i = length - 1; i >= 0; i--)
            {
                ret[i] = (byte)(value % 10);
                value /= 10;
                ret[i] |= (byte)((value % 10) << 4);
                value /= 10;
            }
            return ret;
        }
        private void SetLever(short lever)
        {
            if (savedEP != null)
            {
                client?.SendAsync(new byte[] { (byte)MessageType.SetLever }.Concat(BitConverter.GetBytes(lever)).ToArray(), 3, savedEP);
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
            RequestValues = 5,
            // IO向控制器发送的
            SetLed = 6,
            SetLever = 7,
            // 寻找在线设备
            DokiDoki = 255
        }
    }
}
