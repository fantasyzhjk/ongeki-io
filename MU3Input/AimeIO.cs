using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MU3Input
{
    public static class AimiIO
    {
        [DllExport(CallingConvention = CallingConvention.Cdecl, ExportName = "aime_io_get_api_version")]
        public static ushort GetVersion() => 0x0101;

        [DllExport(CallingConvention = CallingConvention.Cdecl, ExportName = "aime_io_init")]
        public static uint Init()
        {
            if (Process.GetCurrentProcess().ProcessName != "amdaemon" &&
                Process.GetCurrentProcess().ProcessName != "Debug" &&
                Process.GetCurrentProcess().ProcessName != "Test")
                return 1;

            return 0;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl, ExportName = "aime_io_nfc_poll")]
        public static uint Poll(byte unitNumber)
        {
            return 0;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl, ExportName = "aime_io_nfc_get_felica_id")]
        public static unsafe uint GetFelicaId(byte unitNumber, ulong* id)
        {
            if (Mu3IO.IO == null || Mu3IO.IO.Scan != 2)
            {
                return 1;
            }
            else
            {
                ulong val = 0;
                for (int i = 2; i < 10; i++)
                {
                    val = (val << 8) | Mu3IO.IO.AimiId[i];
                }
                *id = val;
                return 0;
            }
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl, ExportName = "aime_io_nfc_get_aime_id")]
        public static uint GetAimeId(byte unitNumber, IntPtr id, ulong size)
        {
            if (Mu3IO.IO == null || Mu3IO.IO.Scan != 1) return 1;

            Marshal.Copy(Mu3IO.IO.AimiId, 0, id, 10);

            return 0;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl, ExportName = "aime_io_led_set_color")]
        public static void SetColor(byte unitNumber, byte r, byte g, byte b)
        {

        }
    }
}
