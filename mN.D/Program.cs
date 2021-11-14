using System;
using System.Threading.Tasks;

namespace mN.D
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var b = new Bot(args[0]);
            await b.RunsAsync(args[1]);
            Console.ReadLine();
            await b.DisconnectAsync();
        }
    }
}
