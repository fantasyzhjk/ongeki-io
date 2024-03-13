// See https://aka.ms/new-console-template for more information
using System.Runtime.InteropServices;
using MU3Input;

ulong id = 0;
ushort code = 0;
byte left = 0;
byte right = 0;
IntPtr aimeid = Marshal.AllocHGlobal(10);
unsafe
{
    Mu3IO.Init();
    AimiIO.Init();
    while (true)
    {
        Task.Delay(1).Wait();
        Mu3IO.Poll();
        Mu3IO.GetGameButtons(&left, &right);
        AimiIO.GetFelicaId(0, &id);
        AimiIO.GetFelicaPm(0, &id);
        AimiIO.GetFelicaSystemCode(0, &code);
        AimiIO.GetAimeId(0, (byte*)aimeid, 10);
    }
}
