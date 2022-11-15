using SimpleHID.Raw;

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace MU3Input
{
    // ReSharper disable once InconsistentNaming
    public class HidIO : IO
    {

        protected int _openCount = 0;
        private byte[] _inBuffer = new byte[64];
        private readonly SimpleRawHID _hid = new SimpleRawHID();
        private const ushort VID = 0x2341;
        private const ushort PID = 0x8036;
        protected OutputData data;


        public HidIO()
        {
            data = new OutputData() { Buttons = new byte[10] };
            Reconnect();
            new Thread(PollThread).Start();
        }
        public override bool IsConnected => _openCount > 0;
        public override OutputData Data => data;

        public override void Reconnect()
        {
            if (IsConnected)
                _hid.Close();

            _openCount = _hid.Open(1, VID, PID);
        }

        public static int[] bitPosMap =
        {
            23, 19, 22, 20, 21, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6
        };


        private unsafe void PollThread()
        {
            while (true)
            {
                if (!IsConnected)
                    continue;

                var len = _hid.Receive(0, ref _inBuffer, 64, 1000);
                if (len < 0)
                {
                    _openCount = 0;
                    _hid.Close();
                    continue;
                }

                OutputData temp = new OutputData();
                temp.Buttons = new ArraySegment<byte>(_inBuffer, 0, 10).ToArray();
                temp.Lever = BitConverter.ToInt16(_inBuffer, 10);
                temp.OptButtons = (OptButtons)_inBuffer[12];
                temp.Aime.Scan = _inBuffer[13];
                if (temp.Aime.Scan == 1)
                {
                    temp.Aime.Mifare = Mifare.Create(new ArraySegment<byte>(_inBuffer, 14, 10).ToArray());
                    bool flag = true;
                    for (int i = 0; i < 10; i++)
                    {
                        if (temp.Aime.Mifare.ID[i] != 255)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        byte[] bytes = Utils.ReadOrCreateAimeTxt();
                        temp.Aime.Mifare = Mifare.Create(bytes);
                    }
                }
                if(temp.Aime.Scan == 2)
                {
                    temp.Aime.Felica.IDm = BitConverter.ToUInt64(_inBuffer, 14);
                    temp.Aime.Felica.PMm = BitConverter.ToUInt64(_inBuffer, 22);
                    temp.Aime.Felica.SystemCode = BitConverter.ToUInt16(_inBuffer, 30);
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
            fixed (void* d = outBuffer)
                Kernel32.CopyMemory(d, &led, 64);

            _hid.Send(0, outBuffer, 64, 1000);
        }


    }
}