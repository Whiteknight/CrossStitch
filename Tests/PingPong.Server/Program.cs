using CrossStitch.Core;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.InMemory;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
using System;

namespace PingPong.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = NodeConfiguration.GetDefault();
            using (var node = new CrossStitchCore(config))
            {
                var dataStorage = new InMemoryDataStorage();

                var ping = new StitchInstance
                {
                    Name = "PingPong.Ping",
                    GroupName = new StitchGroupName("PingPong", "Ping", "1"),
                    Adaptor = new InstanceAdaptorDetails
                    {
                        RunMode = InstanceRunModeType.V1Process
                    },
                    DirectoryPath = ".",
                    ExecutableName = "PingPong.Ping.exe",
                    State = InstanceStateType.Running,
                    LastHeartbeatReceived = 0
                };
                var pong = new StitchInstance
                {
                    Name = "PingPong.Pong",
                    GroupName = new StitchGroupName("PingPong", "Pong", "1"),
                    Adaptor = new InstanceAdaptorDetails
                    {
                        RunMode = InstanceRunModeType.V1Process
                    },
                    DirectoryPath = ".",
                    ExecutableName = "PingPong.Pong.exe",
                    State = InstanceStateType.Running,
                    LastHeartbeatReceived = 0
                };

                dataStorage.Save(ping, true);
                dataStorage.Save(pong, true);

                var data = new DataModule(node.MessageBus, dataStorage);
                node.AddModule(data);

                var stitchesConfiguration = StitchesConfiguration.GetDefault();
                var stitches = new StitchesModule(stitchesConfiguration);
                node.AddModule(stitches);

                var log = Common.Logging.LogManager.GetLogger("CrossStitch");
                var logging = new LoggingModule(log);
                node.AddModule(logging);

                // TODO: We need a way to start for initialization to complete, either having Start
                // block or providing an Initialized event which waits for all modules to report
                // being initialized
                node.Start();

                Console.ReadKey();
                node.Stop();
            }
        }
    }
}
