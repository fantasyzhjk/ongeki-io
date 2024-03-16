using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.Win32;

namespace MU3Input
{
    public static class Mu3IO
    {
        internal static IO IO;
        internal static byte[] LedData;
        private static MemoryMappedFile mmf;
        private static MemoryMappedViewAccessor accessor;

        public static IO CreateIO(IOConfig config)
        {
            if (config.hid is not null)
                return new HidIO(config.hid);
            if (config.kbd is not null)
                return new KeyboardIO(config.kbd);
            if (config.mouse is not null)
                return new MouseIO(config.mouse);
            if (config.tcp is not null)
                return new TcpIO(config.tcp);
            if (config.udp is not null)
                return new UdpIO(config.udp);
            throw new ArgumentException($"Unknown IO type");
        }

        static Mu3IO()
        {
            PInvoke.AllocConsole();
            var io = new MixedIO();
            foreach (var ioConfig in Config.Instance.IO)
            {
                io.Add(CreateIO(ioConfig), ioConfig.Part);
            }
            IO = io;

            //与mod共享内存以接收LED数据
            mmf = MemoryMappedFile.CreateOrOpen("mu3_led_data", 66 * 3);
            accessor = mmf.CreateViewAccessor(0, 66 * 3);
            LedData = new byte[6];
        }

        public static ushort GetVersion()
        {
            return 0x0102;
        }

        public static uint Init()
        {
            string processName = Process.GetCurrentProcess().ProcessName;
            Console.WriteLine(processName);
            if (processName is not "amdaemon" or "Debug" or "Test" or "a")
                return 1;
            else
                return 0;
        }

        public static uint Poll()
        {
            if (IO == null)
                return 0;

            if (!IO.IsConnected)
            {
                IO.Reconnect();
            }

            int leftBase = 0;
            accessor.ReadArray(leftBase, LedData, 0, 3);

            int rightBase = 59 * 3;
            accessor.ReadArray(rightBase, LedData, 3, 3);

            return 0;
        }

        public static unsafe void GetOpButtons(byte* opbtn)
        {
            if (IO == null || !IO.IsConnected)
            {
                *opbtn = 0;
                return;
            }

            *opbtn = (byte)IO.OptButtonsStatus;
        }

        public static unsafe void GetGameButtons(byte* left, byte* right)
        {
            if (IO == null || !IO.IsConnected)
            {
                *left = 0;
                *right = 0;
                return;
            }

            *left = IO.LeftButton;
            *right = IO.RightButton;
        }

        public static unsafe void GetLever(short* pos)
        {
            *pos = 0;
            if (IO == null || !IO.IsConnected)
            {
                *pos = 0;
                return;
            }

            *pos = IO.Lever;
        }

        public static void SetLed(uint data)
        {
            IO.SetLed(data);
            Utils.SetColor(data);
        }
    }
}
