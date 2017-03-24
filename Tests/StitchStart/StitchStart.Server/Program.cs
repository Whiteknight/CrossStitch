using CrossStitch.Core;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.InMemory;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
using CrossStitch.Stitch.ProcessV1;
using System;
using System.Collections.Generic;

namespace StitchStart.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = NodeConfiguration.GetDefault();
            using (var core = new CrossStitchCore(config))
            {
                var dataStorage = new InMemoryDataStorage();

                // First stitch is a ProcessV1 stitch in a separate process
                var result = dataStorage.Save(new StitchInstance
                {
                    Name = "StitchStart.Client",
                    GroupName = new StitchGroupName("StitchStart", "Client", "1"),
                    Adaptor = new InstanceAdaptorDetails
                    {
                        Type = AdaptorType.ProcessV1,
                        Parameters = new Dictionary<string, string>
                        {
                            { Parameters.DirectoryPath, "." },
                            { Parameters.ExecutableName, "StitchStart.Client.exe" }
                        }
                    },
                    State = InstanceStateType.Running,
                    LastHeartbeatReceived = 0
                }, true);

                // Second stitch is a built-in class
                result = dataStorage.Save(new StitchInstance
                {
                    Name = "StitchStart.BuiltIn",
                    GroupName = new StitchGroupName("StitchStart", "BuiltIn", "1"),
                    Adaptor = new InstanceAdaptorDetails
                    {
                        Type = AdaptorType.BuildInClassV1,
                        Parameters = new Dictionary<string, string>
                        {
                            { CrossStitch.Stitch.BuiltInClassV1.Parameters.TypeName, typeof(StitchStartBuiltInStitch).AssemblyQualifiedName }
                        }
                    },
                    State = InstanceStateType.Running,
                    LastHeartbeatReceived = 0
                }, true);

                var data = new DataModule(core.MessageBus, dataStorage);
                core.AddModule(data);

                var stitchesConfiguration = StitchesConfiguration.GetDefault();
                var stitches = new StitchesModule(core, stitchesConfiguration);
                core.AddModule(stitches);

                var log = Common.Logging.LogManager.GetLogger("CrossStitch");
                var logging = new LoggingModule(core, log);
                core.AddModule(logging);

                core.Start();
                Console.ReadKey();
                core.Stop();
            }
        }
    }
}
