using Acquaintance;
using CrossStitch.Core.Data;
using CrossStitch.Core.Modules.Http;
using CrossStitch.Core.Modules.Stitches;
using CrossStitch.Core.Networking.NetMq;
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
            var network = new NetMqNetwork();

            using (var runningNode = new CrossStitchCore(nodeConfig, messageBus))
            {
                var httpConfiguration = HttpConfiguration.GetDefault();
                var httpServer = new NancyHttpModule(httpConfiguration, messageBus);
                runningNode.AddModule(httpServer);

                var dataConfiguration = DataConfiguration.GetDefault();
                var dataStorage = new FolderDataStorage(dataConfiguration);
                var data = new DataModule(dataStorage);
                runningNode.AddModule(data);

                var stitchesConfiguration = StitchesConfiguration.GetDefault();
                var stitches = new StitchesModule(stitchesConfiguration);
                runningNode.AddModule(stitches);

                runningNode.Start();
                Console.ReadKey();
                runningNode.Stop();
            }
        }
    }
}
