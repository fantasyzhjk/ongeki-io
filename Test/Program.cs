using MU3Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal class Program
    {
        static unsafe void Main(string[] args)
        {
            ulong id=0;
            Mu3IO.Init();
            AimiIO.Init();
            while (true)
            {
                Task.Delay(1).Wait();
                Mu3IO.Poll();
                Mu3IO.GetGameButtons(out byte left, out byte right);
                AimiIO.GetFelicaId(0, &id) ;
            }
            Console.ReadKey();
        }
    }
}
