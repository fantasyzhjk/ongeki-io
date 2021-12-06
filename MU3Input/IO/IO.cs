using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MU3Input
{
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
                var value = Math.Pow(_data.Lever / 1023.0, 0.4545) - 0.5;
                return (short)(value * 32766);
            }
        }

        public bool Scan => _data.Scan;

        public byte[] AimiId => _data.AimiId;

        public abstract bool IsConnected { get; }
        public abstract void Reconnect();
        public abstract void SetLed(uint data);
        public abstract void SetAimiId(byte[] id);
    }
}
