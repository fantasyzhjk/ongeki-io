using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MU3Input
{
    internal class MouseIO : IO
    {
        private MouseIOConfig config;
        public override OutputData Data => GetData();

        public override bool IsConnected => true;

        public MouseIO(MouseIOConfig param)
        {
            config = param;
        }

        public override void Reconnect() { }

        public override void SetLed(uint data) { }


        private OutputData GetData()
        {
            User32.GetCursorPos(out var lpPoint);
            var lever = (short)lpPoint.X;
            if (lever < config.Min)
            {
                config.Min = lever;
                Console.WriteLine($"Set lever range: {config.Min}-{config.Max}");
            }
            if (lever > config.Max)
            {
                config.Max = lever;
                Console.WriteLine($"Set lever range: {config.Min}-{config.Max}");
            }
            return new OutputData
            {
                Buttons = new byte[10],
                Lever = lever,
                OptButtons = OptButtons.None,
                Aime = new Aime() { Data = new byte[18] }
            };
        }

 
    }
}
