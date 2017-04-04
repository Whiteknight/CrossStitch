using CrossStitch.Core;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.InMemory;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
using CrossStitch.Http.NancyFx;
using System;
using System.Collections.Generic;

namespace HttpTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var nodeConfig = NodeConfiguration.GetDefault();

            using (var core = new CrossStitchCore(nodeConfig))
            {
                var httpServer = new NancyHttpModule(core.MessageBus);
                core.AddModule(httpServer);

                var dataStorage = new InMemoryDataStorage();
                var data = new DataModule(core.MessageBus, dataStorage);
                core.AddModule(data);

                var stitchesConfiguration = StitchesConfiguration.GetDefault();
                var stitches = new StitchesModule(core, stitchesConfiguration);
                core.AddModule(stitches);

                var groupName = new StitchGroupName("HttpTest", "Stitch", "1");

                var packageFile = new PackageFile
                {
                    Id = groupName.ToString(),
                    GroupName = groupName,
                    Adaptor = new InstanceAdaptorDetails
                    {
                        Type = AdaptorType.ProcessV1,
                        Parameters = new Dictionary<string, string>
                        {
                            { CrossStitch.Stitch.ProcessV1.Parameters.DirectoryPath, "." },
                            { CrossStitch.Stitch.ProcessV1.Parameters.ExecutableName, "HttpTest.Stitch.exe" }
                        }
                    },
                };
                dataStorage.Save(packageFile, true);
                var stitch = new StitchInstance
                {
                    Name = "HttpTest.Stitch",
                    GroupName = groupName,

                    State = InstanceStateType.Running,
                    LastHeartbeatReceived = 0
                };
                dataStorage.Save(stitch, true);

                core.AddModule(new LoggingModule(core, Common.Logging.LogManager.GetLogger("CrossStitch")));

                core.Start();
                Console.ReadKey();
                core.Stop();
            }
        }
    }
}
