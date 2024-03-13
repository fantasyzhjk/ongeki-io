using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace MU3Input
{
    internal class Utils
    {
        public static byte[] ReadOrCreateAimeTxt()
        {
            byte[] aimeId;
            var location = System.AppContext.BaseDirectory;
            string directoryName = Path.GetDirectoryName(location);
            string deviceDirectory = Path.Combine(directoryName, "DEVICE");
            string aimeIdPath = Path.Combine(deviceDirectory, "aime.txt");
            Console.WriteLine("load_aime: {0}", aimeIdPath);
            try
            {
                var id = BigInteger.Parse(File.ReadAllText(aimeIdPath));
                var bytes = id.ToBcd();
                aimeId = new byte[10 - bytes.Length].Concat(bytes).ToArray();
            }
            catch (Exception)
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
    }
}
