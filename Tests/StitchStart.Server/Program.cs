using Acquaintance;
using CrossStitch.Core.Data;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Logging;
using CrossStitch.Core.Modules.Stitches;
using CrossStitch.Core.Node;
using System;

namespace StitchStart.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = NodeConfiguration.GetDefault();
            var messageBus = new MessageBus();
            using (var node = new CrossStitchCore(config, messageBus))
            {
                var dataConfiguration = DataConfiguration.GetDefault();
                var dataStorage = new InMemoryDataStorage(dataConfiguration);

                dataStorage.Save(new StitchInstance
                {
                    Name = "StitchStart.Client",
                    Application = "StitchStart",
                    Component = "Client",
                    Version = "1",
                    Adaptor = new InstanceAdaptorDetails
                    {
                        RunMode = InstanceRunModeType.V1Process
                    },
                    FullName = "StitchStart.Client",
                    DirectoryPath = ".",
                    ExecutableName = "StitchStart.Client.exe",
                    State = InstanceStateType.Running,
                    MissedHeartbeats = 0
                });

                var data = new DataModule(dataStorage);
                node.AddModule(data);

                var stitchesConfiguration = StitchesConfiguration.GetDefault();
                var stitches = new StitchesModule(stitchesConfiguration);
                node.AddModule(stitches);

                var log = Common.Logging.LogManager.GetLogger("CrossStitch");
                var logging = new LoggingModule(log);
                node.AddModule(logging);

                node.Start();
                Console.ReadKey();
                node.Stop();
            }
        }
    }
}
