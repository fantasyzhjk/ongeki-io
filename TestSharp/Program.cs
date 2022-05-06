using MU3Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Mu3IO.Init();
            while (true)
            {
                Task.Delay(1).Wait();
                Mu3IO.Poll();
                Mu3IO.GetGameButtons(out byte left,out byte right);
            }
            Console.ReadKey();
        }
    }
}
