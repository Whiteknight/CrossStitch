using System;
using CrossStitch.Core.Apps;

namespace AppDomainStart.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var adaptor = new AppDomainAppAdaptor(new ComponentInstance(Guid.Empty, Guid.Empty) {
                DirectoryPath = @"C:\Projects\CrossStitch\Tests\AppDomainStart.Client\bin\Debug",
                ExecutableName = "AppDomainStart.Client.exe",
                ApplicationClassName = "MyComponent",
            });
            adaptor.AppInitialized += AdaptorOnAppInitialized;
            bool started = adaptor.Start();
            Console.WriteLine("Started appDomain: " + started);
            Console.ReadKey();
            adaptor.Stop();
        }

        private static void AdaptorOnAppInitialized(object sender, AppStartedEventArgs appStartedEventArgs)
        {
            Console.WriteLine("Started app:" + appStartedEventArgs.InstanceId);
        }
    }
}
