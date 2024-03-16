using SimpleHID.Raw;

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using static MU3Input.KeyboardIO;

namespace MU3Input
{
    public class HidIO : IO
    {
        private HidIOConfig config;
        private short leverDir = 0;
        protected int _openCount = 0;
        private byte[] _inBuffer = new byte[64];
        private readonly SimpleRawHID _hid = new SimpleRawHID();
        protected OutputData data;


        public HidIO(HidIOConfig config)
        {
            this.config = config;
            leverDir = (short)((config.LeverLeft - config.LeverRight < 0) ? -1 : 1);
            data = new OutputData() { Buttons = new byte[10], Aime = new Aime() { Data = new byte[18] } };
            Reconnect();
            new Thread(PollThread).Start();
        }
        public override bool IsConnected => _openCount > 0;
        public override OutputData Data => data;

        public override void Reconnect()
        {
            if (IsConnected)
                _hid.Close();

            _openCount = _hid.Open(1, config.VID, config.PID);
            if (_openCount != 0) {
                Console.WriteLine("已连接 {0}", _openCount);
            }
        }

        public static int[] bitPosMap =
        {
            23, 19, 22, 20, 21, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6
        };

        
        private long map(long x, long in_min, long in_max, long out_min, long out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }


        private unsafe void PollThread()
        {
            while (true)
            {
                if (!IsConnected)
                    continue;

                var len = _hid.Receive(0, ref _inBuffer, 64, 1000);
                if (len < 0)
                {
                    Console.WriteLine("hid设备已断开");
                    _openCount = 0;
                    _hid.Close();
                    continue;
                }

                OutputData temp = new OutputData();
                temp.Buttons = new ArraySegment<byte>(_inBuffer, 0, 10).ToArray();
                short lever;
                lever = BitConverter.ToInt16(_inBuffer, 10);
                if (config.AutoCal)
                {
                    if (leverDir == -1)
                    {
                        if (lever < config.LeverLeft)
                        {
                            config.LeverLeft = lever;
                            Console.WriteLine($"Set lever range: {config.LeverLeft}-{config.LeverRight}");
                        }
                        if (lever > config.LeverRight)
                        {
                            config.LeverRight = lever;
                            Console.WriteLine($"Set lever range: {config.LeverLeft}-{config.LeverRight}");
                        }
                    }
                    if (leverDir == 1)
                    {
                        if (lever > config.LeverLeft)
                        {
                            config.LeverLeft = lever;
                            Console.WriteLine($"Set lever range: {config.LeverLeft}-{config.LeverRight}");
                        }
                        if (lever < config.LeverRight)
                        {
                            config.LeverRight = lever;
                            Console.WriteLine($"Set lever range: {config.LeverLeft}-{config.LeverRight}");
                        }
                    }
                }
                if (config.LeverRight != config.LeverLeft)
                {
                    short leverd = (short)map(lever, config.LeverLeft, config.LeverRight, -20000, 20000);
                    temp.Lever = leverd;
                }
                temp.OptButtons = (OptButtons)_inBuffer[12];
                temp.Aime.Scan = _inBuffer[13];
                if (temp.Aime.Scan == 1)
                {
                    byte[] mifareID = new ArraySegment<byte>(_inBuffer, 14, 10).ToArray();
                    bool flag_FF = true;
                    bool flag_00 = true;
                    for (int i = 0; i < 10; i++)
                    {
                        if (mifareID[i] != 255)
                        {
                            flag_FF = false;
                            break;
                        }

                        if (mifareID[i] != 0)
                        {
                            flag_00 = false;
                            break;
                        }
                    };

                    if (flag_FF || flag_00)
                    {
                        mifareID = Utils.ReadOrCreateAimeTxt();
                    }
                    temp.Aime.ID = mifareID;
                }
                if (temp.Aime.Scan == 2)
                {
                    temp.Aime.IDm = BitConverter.ToUInt64(_inBuffer, 14);
                    temp.Aime.PMm = BitConverter.ToUInt64(_inBuffer, 22);
                    temp.Aime.SystemCode = BitConverter.ToUInt16(_inBuffer, 30);
                }
                data = temp;
            }
        }

        public unsafe override void SetLed(uint data)
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
            var p = &led;
            Marshal.Copy((nint)p, outBuffer, 0, sizeof(SetLedInput));

            _hid.Send(0, outBuffer, 64, 1000);
        }

    }

}