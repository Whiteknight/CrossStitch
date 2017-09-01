using CrossStitch.Core;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
using System;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Messages.Stitches;
using Acquaintance;
using CrossStitch.Core.Messages.Data;
using CrossStitch.Stitch.Process;

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

                // First stitch is a Process, using STDIO
                StartProcessStitch(core, "Stdio", MessageChannelType.Stdio);

                // Second stitch is a Process using Pipes
                StartProcessStitch(core, "Pipes", MessageChannelType.Pipe);

                // Second stitch is a built -in class
                StartBuiltinStitch(core);

                Console.ReadKey();
                core.Stop();
            }
        }

        private static void StartBuiltinStitch(CrossStitchCore core)
        {
            var group3 = new StitchGroupName("StitchStart", "BuiltIn", "1");
            var packageResult3 = core.MessageBus.RequestWait<DataRequest<PackageFile>, DataResponse<PackageFile>>(DataRequest<PackageFile>.Save(new PackageFile
            {
                Id = group3.ToString(),
                GroupName = group3,
                Adaptor = new InstanceAdaptorDetails
                {
                    Type = AdaptorType.BuildInClassV1,
                    Parameters = new Dictionary<string, string>
                    {
                        { CrossStitch.Stitch.BuiltInClassV1.Parameters.TypeName, typeof(StitchStartBuiltInStitch).AssemblyQualifiedName }
                    }
                },
            }, true));
            var createResult3 = core.MessageBus.RequestWait<LocalCreateInstanceRequest, LocalCreateInstanceResponse>(new LocalCreateInstanceRequest
            {
                Name = "StitchStart.BuiltIn",
                GroupName = group3,
                NumberOfInstances = 1
            });
            core.MessageBus.RequestWait<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStart, new InstanceRequest
            {
                Id = createResult3.CreatedIds.FirstOrDefault()
            });
        }

        private static void StartProcessStitch(CrossStitchCore core, string name, MessageChannelType channelType)
        {
            var group1 = new StitchGroupName("StitchStart", name, "1");
            var packageResult1 = core.MessageBus.RequestWait<DataRequest<PackageFile>, DataResponse<PackageFile>>(DataRequest<PackageFile>.Save(new PackageFile
            {
                Id = group1.ToString(),
                GroupName = group1,
                Adaptor = new InstanceAdaptorDetails
                {
                    Type = AdaptorType.ProcessV1,
                    Parameters = new Dictionary<string, string>
                    {
                        { Parameters.RunningDirectory, "." },
                        { Parameters.ExecutableName, "StitchStart.Client.exe" }
                    },
                    RequiresPackageUnzip = false,
                    Channel = channelType
                }
            }, true));
            var createResult1 = core.MessageBus.RequestWait<LocalCreateInstanceRequest, LocalCreateInstanceResponse>(new LocalCreateInstanceRequest
            {
                Name = "StitchStart." + name,
                GroupName = group1,
                NumberOfInstances = 1,
            });
            core.MessageBus.RequestWait<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStart, new InstanceRequest
            {
                Id = createResult1.CreatedIds.FirstOrDefault()
            });
        }
    }
}
