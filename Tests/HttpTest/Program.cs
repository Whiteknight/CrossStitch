using Acquaintance;
using CrossStitch.Core.Data;
using CrossStitch.Core.Http;
using CrossStitch.Core.Node;
using CrossStitch.Http.NancyFx;
using System;

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

                var dataConfiguration = DataConfiguration.GetDefault();
                var dataStorage = new FolderDataStorage(dataConfiguration);
                var data = new DataModule(dataStorage);
                runningNode.AddModule(data);

                runningNode.Start();
                Console.ReadKey();
                runningNode.Stop();
            }
        }
    }
}
