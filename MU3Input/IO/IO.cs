using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MU3Input
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 64)]
    public struct OutputData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10, ArraySubType = UnmanagedType.U1)]
        public byte[] Buttons;

        public short Lever;
        [MarshalAs(UnmanagedType.U1)] public bool Scan;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] AimiId;
        public OptButtons OptButton;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 64)]
    public unsafe struct SetLedInput
    {
        public byte Type;
        public byte LedBrightness;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public fixed byte LedColors[3 * 10];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 64)]
    public unsafe struct SetOptionInput
    {
        public byte Type;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public fixed byte AimiId[10];
    }
    public abstract class IO
    {
        protected OutputData _data;

        public OutputData Data => _data;

        public byte LeftButton =>
            (byte)(_data.Buttons[0] << 0
                    | _data.Buttons[1] << 1
                    | _data.Buttons[2] << 2
                    | _data.Buttons[3] << 3
                    | _data.Buttons[4] << 4);

        public byte RightButton =>
            (byte)(_data.Buttons[5] << 0
                    | _data.Buttons[6] << 1
                    | _data.Buttons[7] << 2
                    | _data.Buttons[8] << 3
                    | _data.Buttons[9] << 4);

        public short Lever
        {
            get
            {
                return _data.Lever;
            }
        }

        public bool Scan => _data.Scan;

        public byte[] AimiId => _data.AimiId;
        public OptButtons OptButtonsStatus => _data.OptButton;

        public abstract bool IsConnected { get; }
        public abstract void Reconnect();
        public abstract void SetLed(uint data);
        public abstract void SetAimiId(byte[] id);
    }
    [Flags]
    public enum OptButtons : byte
    {
        Test = 0b01,
        Service = 0b10
    }
}
