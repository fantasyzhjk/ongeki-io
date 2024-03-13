using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace MU3Input
{
    public class Config
    {
        public static Config Instance = new();
        private static string configPath;

        private Config()
        {
            var location = System.AppContext.BaseDirectory;
            string directoryName = Path.GetDirectoryName(location);
            configPath = Path.Combine(directoryName, "mu3input_config_zhjk.json");
            Console.WriteLine("config_path: {0}", configPath.ToString());
            if (File.Exists(configPath))
            {
                IO = JsonSerializer.Deserialize(
                    File.ReadAllText(configPath),
                    SourceGenerationContext.Default.ListIOConfig
                );
            }
            else
            {
                IO = new List<IOConfig>()
                {
                    new IOConfig() { kbd = new KeyboardIOConfig(), Part = ControllerPart.All },
                    new IOConfig() { hid = new HidIOConfig(), Part = ControllerPart.All },
                    new IOConfig() { tcp = new TCPIOConfig(), Part = ControllerPart.All },
                    new IOConfig() { udp = new UDPIOConfig(), Part = ControllerPart.All },
                };
                Save(configPath);
            }
        }

        public void Save()
        {
            Save(configPath);
        }

        public void Save(string path)
        {
            var json = JsonSerializer.Serialize(
                this.IO,
                SourceGenerationContext.Default.ListIOConfig
            );
            File.WriteAllText(path, json);
        }

        public List<IOConfig> IO { get; set; }
    }

    public class TCPIOConfig
    {
        public string ip { get; set; } = "127.0.0.1";
        public int port { get; set; } = 4300;
    }

    public class UDPIOConfig
    {
        public string ip { get; set; } = "127.0.0.1";
        public int port { get; set; } = 4300;
    }

    public class KeyboardIOConfig
    {
        public int L1 { get; set; } = (int)(-1);
        public int L2 { get; set; } = (int)(-1);
        public int L3 { get; set; } = (int)(-1);
        public int LSide { get; set; } = (int)(-1);
        public int LMenu { get; set; } = (int)(-1);
        public int R1 { get; set; } = (int)(-1);
        public int R2 { get; set; } = (int)(-1);
        public int R3 { get; set; } = (int)(-1);
        public int RSide { get; set; } = (int)(-1);
        public int RMenu { get; set; } = (int)(-1);
        public int Test { get; set; } = (int)(-1);
        public int Service { get; set; } = (int)(-1);
        public int Coin { get; set; } = (int)(-1);
        public int Scan { get; set; } = (int)(-1);
    }

    public class HidIOConfig
    {
        public bool AutoCal { get; set; } = true;
        public short LeverLeft { get; set; } = short.MaxValue;
        public short LeverRight { get; set; } = short.MinValue;
        public bool InvertLever { get; set; } = true;
    }

    public class IOConfig
    {
        public KeyboardIOConfig kbd { get; set; }
        public HidIOConfig hid { get; set; }
        public TCPIOConfig tcp { get; set; }
        public UDPIOConfig udp { get; set; }

        public required ControllerPart Part { get; set; }
    }

    [JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonSerializable(typeof(List<IOConfig>))]
    [JsonSerializable(typeof(IOConfig))]
    [JsonSerializable(typeof(HidIOConfig))]
    [JsonSerializable(typeof(KeyboardIOConfig))]
    [JsonSerializable(typeof(TCPIOConfig))]
    [JsonSerializable(typeof(UDPIOConfig))]
    [JsonSerializable(typeof(int))]
    internal partial class SourceGenerationContext : JsonSerializerContext { }

    [JsonConverter(typeof(JsonStringEnumConverter<ControllerPart>))]
    public enum ControllerPart
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
        ExceptLever = Buttons | Aime,
        All = GamePlay | Menu | Aime,
    }
}
