using System;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core;
using CrossStitch.Core.Messages.Data;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
using Acquaintance;
using CrossStitch.Core.Messages.StitchMonitor;

namespace Monitoring.Server
{
    class Program
    {
        private static void Main(string[] args)
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

                core.MessageBus.Subscribe<StitchHealthEvent>(b => b
                    .WithTopic(StitchHealthEvent.TopicUnhealthy)
                    .Invoke(e => core.Log.LogInformation("Stitch {0} is unhealthy", e.InstanceId)));
                core.MessageBus.Subscribe<StitchHealthEvent>(b => b
                    .WithTopic(StitchHealthEvent.TopicReturnToHealth)
                    .Invoke(e => core.Log.LogInformation("Stitch {0} is Healthy again", e.InstanceId)));

                core.Start();

                StartBuiltinStitch(core);

                Console.ReadKey();
                core.Stop();
            }
        }

        private static void StartBuiltinStitch(CrossStitchCore core)
        {
            var group3 = new StitchGroupName("Monitoring", "BuiltIn", "1");
            var packageResult3 = core.MessageBus.RequestWait<DataRequest<PackageFile>, DataResponse<PackageFile>>(DataRequest<PackageFile>.Save(new PackageFile
            {
                Id = group3.ToString(),
                GroupName = group3,
                Adaptor = new InstanceAdaptorDetails
                {
                    Type = AdaptorType.BuildInClassV1,
                    Parameters = new Dictionary<string, string>
                    {
                        { CrossStitch.Stitch.BuiltInClassV1.Parameters.TypeName, typeof(MonitoringBuiltInStitch).AssemblyQualifiedName }
                    }
                },
            }, true));
            var createResult3 = core.MessageBus.RequestWait<LocalCreateInstanceRequest, LocalCreateInstanceResponse>(new LocalCreateInstanceRequest
            {
                Name = "Monitoring.BuiltIn",
                GroupName = group3,
                NumberOfInstances = 1
            });
            core.MessageBus.RequestWait<InstanceRequest, InstanceResponse>(InstanceRequest.ChannelStart, new InstanceRequest
            {
                Id = createResult3.CreatedIds.FirstOrDefault()
            });
        }
    }
}
