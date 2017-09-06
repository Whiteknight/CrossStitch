using CrossStitch.Core;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.InMemory;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
using System;
using System.Collections.Generic;
using CrossStitch.Stitch.Process;
using Microsoft.Extensions.Logging;

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
                            { Parameters.RunningDirectory, "." },
                            { Parameters.ExecutableName, "JsStitch.Stitch.js" }
                        }
                    },
                }, true);
                dataStorage.Save(new StitchInstance
                {
                    Name = "JsStitch.Stitch",
                    GroupName = groupName,
                    State = InstanceStateType.Running
                }, true);

                var data = new DataModule(core.MessageBus, dataStorage);
                core.AddModule(data);

                var stitchesConfiguration = StitchesConfiguration.GetDefault();
                var stitches = new StitchesModule(core, stitchesConfiguration);
                core.AddModule(stitches);

                var logger = new LoggerFactory().AddConsole(LogLevel.Debug).CreateLogger<Program>();
                core.AddModule(new LoggingModule(core, logger));

                core.Start();
                Console.ReadKey();
                core.Stop();
            }
        }
    }
}
