using System;
using System.Text;

namespace MU3Input
{
    public class User32
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();


        [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowText")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int maxCount);
    }
}