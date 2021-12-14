using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.IO;

namespace MU3Input
{
    public static class Mu3IO
    {
        internal static IO Io;
        public static IOTest _test;

        [DllExport(ExportName = "mu3_io_get_api_version")]
        public static ushort GetVersion()
        {
            return 0x0102;
        }

        [DllExport(CallingConvention.Cdecl, ExportName = "mu3_io_init")]
        public static uint Init()
        {
            if (Process.GetCurrentProcess().ProcessName != "amdaemon" &&
                Process.GetCurrentProcess().ProcessName != "Debug" &&
                Process.GetCurrentProcess().ProcessName != "TestSharp" &&
                Process.GetCurrentProcess().ProcessName != "Test")
                return 0;

            switch (GetIOType().ToLower())
            {
                case "udp":
                    Io = new UdpIO(GetPort());
                    break;
                default:
                    Io = new HidIO();
                    break;
            }

            _test = new IOTest(Io);

            Task.Run(() => _test.ShowDialog());
            return 0;
        }

        [DllExport(CallingConvention.Cdecl, ExportName = "mu3_io_poll")]
        public static uint Poll()
        {
            if (Io == null)
                return 0;

            if (!Io.IsConnected)
            {
                Io.Reconnect();
            }

            _test.UpdateData();
            return 0;
        }

        [DllExport(CallingConvention.Cdecl, ExportName = "mu3_io_get_opbtns")]
        public static void GetOpButtons(out byte opbtn)
        {
            if (Io == null || !Io.IsConnected)
            {
                opbtn = 0;
                return;
            }

            opbtn = Io.OptButton;
        }

        [DllExport(CallingConvention.Cdecl, ExportName = "mu3_io_get_gamebtns")]
        public static void GetGameButtons(out byte left, out byte right)
        {
            if (Io == null || !Io.IsConnected)
            {
                left = 0;
                right = 0;
                return;
            }

            left = Io.LeftButton;
            right = Io.RightButton;
        }

        [DllExport(CallingConvention.Cdecl, ExportName = "mu3_io_get_lever")]
        public static void GetLever(out short pos)
        {
            pos = 0;
            if (Io == null || !Io.IsConnected)
            {
                pos = 0;
                return;
            }

            pos = Io.Lever;
        }

        [DllExport(CallingConvention.Cdecl, ExportName = "mu3_io_set_led")]
        public static void SetLed(uint data)
        {
            _test.SetColor(data);
            Io.SetLed(data);
        }

        const string defaultIOType = "hid";
        public static string GetIOType()
        {
            var location = typeof(Mu3IO).Assembly.Location;
            string directoryName = Path.GetDirectoryName(location);
            string segatoolsIniPath = Path.Combine(directoryName, "segatools.ini");
            if (File.Exists(segatoolsIniPath))
            {
                StringBuilder temp = new StringBuilder();
                GetPrivateProfileString("mu3io", "protocol", defaultIOType, temp, 1024, segatoolsIniPath);
                return temp.ToString();
            }
            return defaultIOType;
        }

        const int defaultPort = 4354;
        public static int GetPort()
        {
            var location = typeof(Mu3IO).Assembly.Location;
            string directoryName = Path.GetDirectoryName(location);
            string segatoolsIniPath = Path.Combine(directoryName, "segatools.ini");
            if (File.Exists(segatoolsIniPath))
            {
                StringBuilder temp = new StringBuilder();
                GetPrivateProfileString("mu3io", "port", defaultPort.ToString(), temp, 1024, segatoolsIniPath);
                if (int.TryParse(temp.ToString(), out int port))
                {
                    return port;
                }
            }
            return defaultPort;
        }

        [DllImport("kernel32")]//返回取得字符串缓冲区的长度
        public static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
    }
}
