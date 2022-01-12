using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace MU3Input
{
    internal class Utils
    {
        public static byte[] ReadOrCreateAimeTxt()
        {
            byte[] aimeId;
            var location = Assembly.GetCallingAssembly().Location;
            string directoryName = Path.GetDirectoryName(location);
            string deviceDirectory = Path.Combine(directoryName, "DEVICE");
            string aimeIdPath = Path.Combine(deviceDirectory, "aime.txt");
            try
            {
                var id = BigInteger.Parse(File.ReadAllText(aimeIdPath));
                var bytes = id.ToBcd();
                aimeId = new byte[10 - bytes.Length].Concat(bytes).ToArray();
            }
            catch (Exception ex)
            {
                Random random = new Random();
                byte[] temp = new byte[10];
                random.NextBytes(temp);
                var id = new BigInteger(temp);
                if (id < -1) id = -(id + 1);
                id = id % BigInteger.Parse("99999999999999999999");
                if (!Directory.Exists(deviceDirectory))
                {
                    Directory.CreateDirectory(deviceDirectory);
                }
                var bytes = id.ToBcd();
                aimeId = new byte[10 - bytes.Length].Concat(bytes).ToArray();
                File.WriteAllText(aimeIdPath, id.ToString());
            }
            return aimeId;
        }

        const string defaultIOType = "hid";
        public static string GetProtocol()
        {
            var location = typeof(Mu3IO).Assembly.Location;
            string directoryName = Path.GetDirectoryName(location);
            string segatoolsIniPath = Path.Combine(directoryName, "segatools.ini");
            if (File.Exists(segatoolsIniPath))
            {
                StringBuilder temp = new StringBuilder();
                Kernel32.GetPrivateProfileString("mu3io", "protocol", defaultIOType, temp, 1024, segatoolsIniPath);
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
                Kernel32.GetPrivateProfileString("mu3io", "port", defaultPort.ToString(), temp, 1024, segatoolsIniPath);
                if (int.TryParse(temp.ToString(), out int port))
                {
                    return port;
                }
            }
            return defaultPort;
        }
    }
}
