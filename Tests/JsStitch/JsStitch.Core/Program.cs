using CrossStitch.Core;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.InMemory;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
using System;
using System.Collections.Generic;
using CrossStitch.Stitch.Process;

namespace JsStitch.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = NodeConfiguration.GetDefault();
            using (var core = new CrossStitchCore(config))
            {
                var dataStorage = new InMemoryDataStorage();
                var groupName = new StitchGroupName("JsStitch", "Stitch", "1");
                dataStorage.Save(new PackageFile
                {
                    Id = groupName.ToString(),
                    GroupName = groupName,
                    Adaptor = new InstanceAdaptorDetails
                    {
                        Type = AdaptorType.ProcessV1,
                        Parameters = new Dictionary<string, string>
                        {
                            { Parameters.DirectoryPath, "." },
                            { Parameters.ExecutableName, "JsStitch.Stitch.js" }
                        }
                    },
                }, true);
                dataStorage.Save(new StitchInstance
                {
                    Name = "JsStitch.Stitch",
                    GroupName = groupName,
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
