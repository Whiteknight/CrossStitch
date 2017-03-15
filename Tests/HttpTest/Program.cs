using CrossStitch.Core;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.Folders;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
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

                var dataConfiguration = Configuration.GetDefault();
                var dataStorage = new FolderDataStorage(dataConfiguration);
                var data = new DataModule(core.MessageBus, dataStorage);
                core.AddModule(data);

                var stitchesConfiguration = StitchesConfiguration.GetDefault();
                var stitches = new StitchesModule(stitchesConfiguration);
                core.AddModule(stitches);

                core.AddModule(new LoggingModule(Common.Logging.LogManager.GetLogger("CrossStitch")));

                core.Start();
                Console.ReadKey();
                core.Stop();
            }
        }
    }
}
