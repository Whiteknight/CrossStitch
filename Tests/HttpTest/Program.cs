using System;
using CrossStitch.Core.Http;
using Acquaintance;
using CrossStitch.Core.Node;
using CrossStitch.Http.NancyFx;

namespace HttpTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var nodeConfig = NodeConfiguration.GetDefault();
            var messageBus = new MessageBus();

            using (var runningNode = new RunningNode(nodeConfig, messageBus))
            {
                var httpConfiguration = HttpConfiguration.GetDefault();
                var httpServer = new NancyHttpModule(httpConfiguration, messageBus);
                runningNode.AddModule(httpServer);

                runningNode.Start();
                Console.ReadKey();
                runningNode.Stop();
            }
        }
    }
}
