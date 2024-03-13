using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MU3Input
{
    public static class AimiIO
    {
        public static ushort GetVersion() => 0x0200;

        public static uint Init()
        {
            string processName = Process.GetCurrentProcess().ProcessName;
            Console.WriteLine(processName);
            if (processName is not "amdaemon" or "Debug" or "Test" or "a")
                return 1;
            else
                return 0;
        }

        public static uint Poll(byte unitNumber)
        {
            return 0;
        }

        public static unsafe uint GetFelicaId(byte unitNumber, ulong* id)
        {
            if (Mu3IO.IO == null || Mu3IO.IO.Aime.Scan != 2)
            {
                return 1;
            }
            else
            {
                *id = Mu3IO.IO.Aime.IDm;
                return 0;
            }
        }

        public static unsafe uint GetFelicaPm(byte unitNumber, ulong* pm)
        {
            if (Mu3IO.IO == null || Mu3IO.IO.Aime.Scan != 2)
            {
                return 1;
            }
            else
            {
                *pm = Mu3IO.IO.Aime.PMm;
                return 0;
            }
        }

        public static unsafe uint GetFelicaSystemCode(byte unitNumber, ushort* systemCode)
        {
            if (Mu3IO.IO == null || Mu3IO.IO.Aime.Scan != 2)
            {
                return 1;
            }
            else
            {
                *systemCode = Mu3IO.IO.Aime.SystemCode;
                return 0;
            }
        }

        public static unsafe uint GetAimeId(byte unitNumber, byte* id, ulong size)
        {
            if (Mu3IO.IO == null || Mu3IO.IO.Aime.Scan != 1)
                return 1;
            Aime aime = Mu3IO.IO.Aime;
            for (int i = 0; i < 10; i++)
            {
                id[i] = aime.ID[i];
            }

            return 0;
        }

        public static void SetColor(byte unitNumber, byte r, byte g, byte b) { }
    }
}
