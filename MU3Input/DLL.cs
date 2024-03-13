using System.Runtime.InteropServices;

namespace MU3Input
{
    public static class Dll
    {
        public static class MU3Input
        {
            [UnmanagedCallersOnly(EntryPoint = "mu3_io_get_api_version")]
            public static ushort GetVersion() => Mu3IO.GetVersion();

            [UnmanagedCallersOnly(EntryPoint = "mu3_io_init")]
            public static uint Init() => Mu3IO.Init();

            [UnmanagedCallersOnly(EntryPoint = "mu3_io_poll")]
            public static uint Poll() => Mu3IO.Poll();

            [UnmanagedCallersOnly(EntryPoint = "mu3_io_get_opbtns")]
            public static unsafe void GetOpButtons(byte* opbtn) => Mu3IO.GetOpButtons(opbtn);

            [UnmanagedCallersOnly(EntryPoint = "mu3_io_get_gamebtns")]
            public static unsafe void GetGameButtons(byte* left, byte* right) =>
                Mu3IO.GetGameButtons(left, right);

            [UnmanagedCallersOnly(EntryPoint = "mu3_io_get_lever")]
            public static unsafe void GetLever(short* pos) => Mu3IO.GetLever(pos);

            [UnmanagedCallersOnly(EntryPoint = "mu3_io_set_led")]
            public static void SetLed(uint data) => Mu3IO.SetLed(data);
        }

        public static class Aimi
        {
            [UnmanagedCallersOnly(EntryPoint = "aime_io_get_api_version")]
            public static ushort GetVersion() => AimiIO.GetVersion();

            [UnmanagedCallersOnly(EntryPoint = "aime_io_init")]
            public static uint Init() => AimiIO.Init();

            [UnmanagedCallersOnly(EntryPoint = "aime_io_nfc_poll")]
            public static uint Poll(byte unitNumber) => AimiIO.Poll(unitNumber);

            [UnmanagedCallersOnly(EntryPoint = "aime_io_nfc_get_felica_id")]
            public static unsafe uint GetFelicaId(byte unitNumber, ulong* id) =>
                AimiIO.GetFelicaId(unitNumber, id);

            [UnmanagedCallersOnly(EntryPoint = "aime_io_nfc_get_felica_pm")]
            public static unsafe uint GetFelicaPm(byte unitNumber, ulong* pm) =>
                AimiIO.GetFelicaPm(unitNumber, pm);

            [UnmanagedCallersOnly(EntryPoint = "aime_io_nfc_get_felica_system_code")]
            public static unsafe uint GetFelicaSystemCode(byte unitNumber, ushort* systemCode) =>
                AimiIO.GetFelicaSystemCode(unitNumber, systemCode);

            [UnmanagedCallersOnly(EntryPoint = "aime_io_nfc_get_aime_id")]
            public static unsafe uint GetAimeId(byte unitNumber, byte* id, ulong size) =>
                AimiIO.GetAimeId(unitNumber, id, size);

            [UnmanagedCallersOnly(EntryPoint = "aime_io_led_set_color")]
            public static void SetColor(byte unitNumber, byte r, byte g, byte b) =>
                AimiIO.SetColor(unitNumber, r, g, b);
        }
    }
}
