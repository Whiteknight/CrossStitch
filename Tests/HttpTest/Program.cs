using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Http;
using CrossStitch.Core.Modules.Stitches;
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

            using (var core = new CrossStitchCore(nodeConfig))
            {
                var httpConfiguration = HttpConfiguration.GetDefault();
                var httpServer = new NancyHttpModule(httpConfiguration, core.MessageBus);
                core.AddModule(httpServer);

                var dataConfiguration = DataConfiguration.GetDefault();
                var dataStorage = new FolderDataStorage(dataConfiguration);
                var data = new DataModule(dataStorage);
                core.AddModule(data);

                var stitchesConfiguration = StitchesConfiguration.GetDefault();
                var stitches = new StitchesModule(stitchesConfiguration);
                core.AddModule(stitches);

                core.Start();
                Console.ReadKey();
                core.Stop();
            }
        }
    }
}
