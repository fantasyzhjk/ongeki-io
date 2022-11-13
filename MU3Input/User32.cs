using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MU3Input
{
    public class User32
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();


        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowText")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int maxCount);

        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetAsyncKeyState")]
        public static extern int GetAsyncKeyState(Keys vKey);
    }
}