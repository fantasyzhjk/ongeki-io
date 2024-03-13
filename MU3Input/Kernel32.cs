﻿using System.Runtime.InteropServices;
using System.Text;

namespace MU3Input
{
    public class Kernel32
    {
        [DllImport("kernel32.dll")]
        public static extern long GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern unsafe void CopyMemory(void* dest, void* src, int count);
        [DllImport("kernel32.dll")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();//显示控制台
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole(); //释放控制台、关闭控制台
    }
}
