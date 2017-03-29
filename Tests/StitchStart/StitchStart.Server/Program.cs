using CrossStitch.Core;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.InMemory;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
using CrossStitch.Stitch.ProcessV1;
using System;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Messages.Stitches;
using Acquaintance;

namespace StitchStart.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = NodeConfiguration.GetDefault();
            using (var core = new CrossStitchCore(config))
            {
                var stitchesConfiguration = StitchesConfiguration.GetDefault();
                var stitches = new StitchesModule(core, stitchesConfiguration);
                core.AddModule(stitches);

                var log = Common.Logging.LogManager.GetLogger("CrossStitch");
                var logging = new LoggingModule(core, log);
                core.AddModule(logging);

                core.Start();

                // First stitch is a p
                var createResult1 = core.MessageBus.Request<LocalCreateInstanceRequest, LocalCreateInstanceResponse>(new LocalCreateInstanceRequest
                {
                    Name = "StitchStart.Client",
                    Adaptor = new InstanceAdaptorDetails
                    {
                        Type = AdaptorType.ProcessV1,
                        Parameters = new Dictionary<string, string>
                        {
                            { Parameters.DirectoryPath, "." },
                            { Parameters.ExecutableName, "StitchStart.Client.exe" }
                        },
                        RequiresPackageUnzip = false
                    },
                    GroupName = new StitchGroupName("StitchStart", "Client", "1"),
                    NumberOfInstances = 1,
                });
                core.MessageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStart, new InstanceRequest
                {
                    Id = createResult1.CreatedIds.FirstOrDefault()
                });

                // Second stitch is a built-in class
                var createResult2 = core.MessageBus.Request<LocalCreateInstanceRequest, LocalCreateInstanceResponse>(new LocalCreateInstanceRequest
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
                    NumberOfInstances = 1
                });
                core.MessageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStart, new InstanceRequest
                {
                    Id = createResult2.CreatedIds.FirstOrDefault()
                });

                Console.ReadKey();
                core.Stop();
            }
        }
    }
}
