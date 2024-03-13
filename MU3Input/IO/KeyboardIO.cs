﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MU3Input
{
    internal class KeyboardIO : IO
    {
        private KeyboardIOConfig config;
        public override OutputData Data => GetData();

        public override bool IsConnected => true;

        public KeyboardIO(KeyboardIOConfig param)
        {
            config = param;
        }

        public override void Reconnect() { }

        public override void SetLed(uint data) { }


        // StringBuilder sb = new StringBuilder();
        // bool coinAvailable = true;
        private OutputData GetData()
        {
            // IntPtr handle = User32.GetForegroundWindow();
            // User32.GetWindowText(handle, sb, 16);
            // string windowText = sb.ToString();
            // if (windowText != "Otoge" && windowText != "Ongeki IO Debug")
            // {
            //     return new OutputData() { Buttons = new byte[10], Aime = new Aime() { Data = new byte[18] } };
            // }

            byte[] buttons = new byte[] {
                Pressed(config.L1),
                Pressed(config.L2),
                Pressed(config.L3),
                Pressed(config.LSide),
                Pressed(config.LMenu),
                Pressed(config.R1),
                Pressed(config.R2),
                Pressed(config.R3),
                Pressed(config.RSide),
                Pressed(config.RMenu),
            };
            short lever = 0;
            byte testPressed = Pressed(config.Test);
            byte servicePressed = Pressed(config.Service);
            byte coinPressed = Pressed(config.Coin);
            // if (coinPressed > 0)
            // {
            //     if (coinAvailable)
            //     {
            //         coinAvailable = false;
            //     }
            //     else
            //     {
            //         coinPressed = 0;
            //     }
            // }
            // else
            // {
            //     coinAvailable = true;
            // }
            // OptButtons optButtons = (OptButtons)(testPressed << 0 | servicePressed << 1 | coinPressed << 2);
            OptButtons optButtons = OptButtons.None;
            if (testPressed > 0) optButtons |= OptButtons.Test;
            if (servicePressed > 0) optButtons |= OptButtons.Service;
            if (coinPressed > 0) optButtons |= OptButtons.Coin;
            Aime aime = new()
            {
                Scan = Pressed(config.Scan),
                Data = new byte[18]
            };
            if (aime.Scan == 1)
            {
                byte[] bytes = Utils.ReadOrCreateAimeTxt();
                aime.ID = bytes;
            }
            return new OutputData
            {
                Buttons = buttons,
                Lever = lever,
                OptButtons = optButtons,
                Aime = aime
            };
        }
        private byte Pressed(int key)
        {
            return User32.GetAsyncKeyState(key) == 0 ? (byte)0 : (byte)1;
        }
 
    }
}
