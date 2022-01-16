using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MU3Input
{
    public static class Initialization
    {
        static Initialization()
        {
            var location = typeof(Mu3IO).Assembly.Location;
            string directoryName = Path.GetDirectoryName(location);
            initializationFilePath = Path.Combine(directoryName, "segatools.ini");
        }
        static StringBuilder temp = new StringBuilder();
        static string initializationFilePath;

        public static class MU3IO
        {
            static string section = "mu3io";
            const string defaultIOType = "hid";
            const int defaultPort = 4354;
            public static string Protocol
            {
                get
                {
                    Kernel32.GetPrivateProfileString(section, nameof(Protocol).ToLower(), defaultIOType, temp, 64, initializationFilePath);
                    return temp.ToString();
                }
            }
            public static int Port
            {
                get
                {
                    Kernel32.GetPrivateProfileString(section, nameof(Port).ToLower(), defaultPort.ToString(), temp, 5, initializationFilePath);
                    if (int.TryParse(temp.ToString(), out int port)) return port;
                    else return defaultPort;
                }
            }

        }
        public static class Overlay
        {
            static string section = "overlay";
            static bool defaultEnabled = false;
            static int defaultX, defaultY, defaultWidth, defaultHeight;
            static Overlay()
            {
                Rectangle rect = Screen.GetBounds(Control.MousePosition);
                defaultX = rect.Width / 2;
                defaultY = rect.Height;
                defaultWidth = 800;
                defaultHeight = 300;
            }
            public static bool Enabled
            {
                get
                {
                    Kernel32.GetPrivateProfileString(section, nameof(Enabled).ToLower(), defaultEnabled.ToString(), temp, 5, initializationFilePath);
                    if (bool.TryParse(temp.ToString(), out bool enabled)) return enabled;
                    else return defaultEnabled;
                }
                set => Kernel32.WritePrivateProfileString(section, nameof(Enabled).ToLower(), value.ToString(), initializationFilePath);
            }
            public static int X
            {
                get
                {
                    Kernel32.GetPrivateProfileString(section, nameof(X).ToLower(), defaultX.ToString(), temp, 5, initializationFilePath);
                    if (int.TryParse(temp.ToString(), out int x)) return x;
                    else return defaultX;
                }
                set => Kernel32.WritePrivateProfileString(section, nameof(X).ToLower(), value.ToString(), initializationFilePath);
            }
            public static int Y
            {
                get
                {
                    Kernel32.GetPrivateProfileString(section, nameof(Y).ToLower(), defaultY.ToString(), temp, 5, initializationFilePath);
                    if (int.TryParse(temp.ToString(), out int y)) return y;
                    else return defaultY;
                }
                set => Kernel32.WritePrivateProfileString(section, nameof(Y).ToLower(), value.ToString(), initializationFilePath);
            }
            public static int Width
            {
                get
                {
                    Kernel32.GetPrivateProfileString(section, nameof(Width).ToLower(), defaultWidth.ToString(), temp, 5, initializationFilePath);
                    if (int.TryParse(temp.ToString(), out int width)) return width;
                    else return defaultWidth;
                }
                set => Kernel32.WritePrivateProfileString(section, nameof(Width).ToLower(), value.ToString(), initializationFilePath);
            }
            public static int Height
            {
                get
                {
                    Kernel32.GetPrivateProfileString(section, nameof(Height).ToLower(), defaultHeight.ToString(), temp, 5, initializationFilePath);
                    if (int.TryParse(temp.ToString(), out int height)) return height;
                    else return defaultHeight;
                }
                set => Kernel32.WritePrivateProfileString(section, nameof(Height).ToLower(), value.ToString(), initializationFilePath);
            }
        }
    }
}
