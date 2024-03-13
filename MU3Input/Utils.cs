using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace MU3Input
{
    internal class Utils
    {
        public static void SetColor(uint data) {
            Console.Write(
                "SetColor:\n{0} {1} {2}, {3} {4} {5}, {6} {7} {8}, {9} {10} {11}, {12} {13} {14}, {15} {16} {17}\n",
                (int)((data >> 23) & 1) * 255,
                (int)((data >> 19) & 1) * 255,
                (int)((data >> 22) & 1) * 255,
                (int)((data >> 20) & 1) * 255,
                (int)((data >> 21) & 1) * 255,
                (int)((data >> 18) & 1) * 255,
                (int)((data >> 17) & 1) * 255,
                (int)((data >> 16) & 1) * 255,
                (int)((data >> 15) & 1) * 255,
                (int)((data >> 14) & 1) * 255,
                (int)((data >> 13) & 1) * 255,
                (int)((data >> 12) & 1) * 255,
                (int)((data >> 11) & 1) * 255,
                (int)((data >> 10) & 1) * 255,
                (int)((data >> 9) & 1) * 255,
                (int)((data >> 8) & 1) * 255,
                (int)((data >> 7) & 1) * 255,
                (int)((data >> 6) & 1) * 255
            );
        }

        public static Nullable<BigInteger> AimeIDLocal = null;
        public static byte[] ReadOrCreateAimeTxt()
        {
            if (AimeIDLocal is not null) return AimeIDLocal.Value.ToBcd();
            byte[] aimeId;
            var location = System.AppContext.BaseDirectory;
            string directoryName = Path.GetDirectoryName(location);
            string deviceDirectory = Path.Combine(directoryName, "DEVICE");
            string aimeIdPath = Path.Combine(deviceDirectory, "aime.txt");
            // Console.WriteLine("load_aime: {0}", aimeIdPath);
            try
            {
                var id = BigInteger.Parse(File.ReadAllText(aimeIdPath));
                AimeIDLocal = id;
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
