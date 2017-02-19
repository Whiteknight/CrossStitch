using System;

namespace AppDomainStart.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var stitch = new TestStitch();
            stitch.Start();
            Console.ReadKey();
            stitch.Stop();
        }
    }
}
