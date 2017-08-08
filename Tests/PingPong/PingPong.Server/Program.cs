using CrossStitch.Core;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.InMemory;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
using System;
using System.Collections.Generic;
using CrossStitch.Stitch.Process;

namespace PingPong.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = NodeConfiguration.GetDefault();
            using (var core = new CrossStitchCore(config))
            {
                var dataStorage = new InMemoryDataStorage();

                var pingGroup = new StitchGroupName("PingPong", "Ping", "1");
                var pingPackage = new PackageFile
                {
                    Id = pingGroup.ToString(),
                    GroupName = pingGroup,
                    Adaptor = new InstanceAdaptorDetails
                    {
                        Type = AdaptorType.ProcessV1,
                        Parameters = new Dictionary<string, string>
                        {
                            { Parameters.DirectoryPath, "." },
                            { Parameters.ExecutableName, "PingPong.Ping.exe" }
                        }
                    },
                };
                var ping = new StitchInstance
                {
                    Name = "PingPong.Ping",
                    GroupName = pingGroup,
                    State = InstanceStateType.Running
                };

                var pongGroup = new StitchGroupName("PingPong", "Pong", "1");
                var pongPackage = new PackageFile
                {
                    Id = pongGroup.ToString(),
                    GroupName = pongGroup,
                    Adaptor = new InstanceAdaptorDetails
                    {
                        Type = AdaptorType.ProcessV1,
                        Parameters = new Dictionary<string, string>
                        {
                            { Parameters.DirectoryPath, "." },
                            { Parameters.ExecutableName, "PingPong.Pong.exe" }
                        }
                    },
                };
                var pong = new StitchInstance
                {
                    Name = "PingPong.Pong",
                    GroupName = pongGroup,
                    State = InstanceStateType.Running
                };

                dataStorage.Save(pingPackage, true);
                dataStorage.Save(ping, true);
                dataStorage.Save(pongPackage, true);
                dataStorage.Save(pong, true);

                var data = new DataModule(core.MessageBus, dataStorage);
                core.AddModule(data);

                var stitchesConfiguration = StitchesConfiguration.GetDefault();
                var stitches = new StitchesModule(core, stitchesConfiguration);
                core.AddModule(stitches);

                var log = Common.Logging.LogManager.GetLogger("CrossStitch");
                var logging = new LoggingModule(core, log);
                core.AddModule(logging);

                // TODO: We need a way to start for initialization to complete, either having Start
                // block or providing an Initialized event which waits for all modules to report
                // being initialized
                core.Start();

                Console.ReadKey();
                core.Stop();
            }
        }
    }
}
