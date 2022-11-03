using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MU3Input
{
    public class Config
    {
        public static Config Instance;
        private static string configPath;
        static Config()
        {
            var location = typeof(Mu3IO).Assembly.Location;
            string directoryName = Path.GetDirectoryName(location);
            configPath = Path.Combine(directoryName, "mu3input_config.json");
            if (Directory.Exists(configPath))
            {
                Instance = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath),new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });
            }
            else
            {
                Instance = new Config();
                Instance.IO = new List<IOConfig>()
                {
                    new IOConfig()
                    {
                        Type = IOType.Udp,
                        Param = "4354",
                        Part = ControllerPart.All
                    }
                };
                Instance.Save(configPath);
            }
        }
        public void Save()
        {
            Save(configPath);
        }
        public void Save(string path)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions() 
            {
                WriteIndented = true, 
                PropertyNameCaseInsensitive = true ,
            }));
        }
        private Config() { }
        public List<IOConfig> IO { get; set; }
    }
    public class IOConfig
    {
        public IOType Type { get; set; }
        public string Param { get; set; }
        public ControllerPart Part { get; set; }
    }


    public enum IOType
    {
        Hid, Udp, Tcp, Usbmux
    }

    [Flags]
    public enum ControllerPart : long
    {
        None = 0,
        L1 = 1 << 0,
        L2 = 1 << 1,
        L3 = 1 << 2,
        LSide = 1 << 3,
        LMenu = 1 << 4,
        R1 = 1 << 5,
        R2 = 1 << 6,
        R3 = 1 << 7,
        RSide = 1 << 8,
        RMenu = 1 << 9,
        Lever = 1 << 10,
        Aime = 1 << 11,
        LKeyBoard = L1 | L2 | L3,
        RKeyBoard = R1 | R2 | R3,
        Side = LSide | RSide,
        Menu = LMenu | RMenu,
        KeyBoard = LKeyBoard | RKeyBoard,
        Left = LKeyBoard | LSide | LMenu,
        Right = RKeyBoard | RSide | RMenu,
        GameButtons = KeyBoard | Side,
        Buttons = GameButtons | Menu,
        GamePlay = GameButtons | Lever,
        All = GamePlay | Aime,
    }
}
