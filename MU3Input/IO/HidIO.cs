using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using SimpleHID.Raw;

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


        public HidIO()
        {
            _data = new OutputData() { Buttons = new byte[10], AimiId = new byte[10] };
            Reconnect();
            new Thread(PollThread).Start();
        }

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

        public override bool IsConnected => _openCount > 0;

        private void PollThread()
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

                var temp = _inBuffer.ToStructure<OutputData>();

                if (temp.AimiId.All(n => n == 255))
                {
                    temp.AimiId = Utils.ReadOrCreateAimeTxt();
                }
                _data = temp;
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
                CopyMemory(d, &led, 64);

            _hid.Send(0, outBuffer, 64, 1000);
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

            _hid.Send(0, outBuffer, 64, 1000);
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern unsafe void CopyMemory(void* dest, void* src, int count);

    }
}