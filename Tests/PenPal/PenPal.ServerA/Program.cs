using System;
using System.Collections.Generic;
using CrossStitch.Backplane.Zyre;
using CrossStitch.Core;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.InMemory;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
using CrossStitch.Stitch.ProcessV1;

namespace PenPal.ServerA
{
    public class Program
    {
        static void Main(string[] args)
        {
            var nodeConfig = NodeConfiguration.GetDefault();
            using (var core = new CrossStitchCore(nodeConfig))
            {
                // Setup an in-memory data store
                var store = new InMemoryDataStorage();
                core.AddModule(new DataModule(core.MessageBus, store));

                // Backplane so we can cluster
                core.AddModule(new BackplaneModule(core));

                // Stitches so we can host the stitch instances
                var stitchesConfig = StitchesConfiguration.GetDefault();
                core.AddModule(new StitchesModule(core, stitchesConfig));

                // Setup logging.
                core.AddModule(new LoggingModule(core, Common.Logging.LogManager.GetLogger("CrossStitch")));

                // Create a stitch instance to run on startup
                var groupName = new StitchGroupName("PenPal.StitchA.1");
                var packageFile = new PackageFile
                {
                    Id = groupName.ToString(),
                    GroupName = groupName,
                    Adaptor = new InstanceAdaptorDetails
                    {
                        Type = AdaptorType.ProcessV1,
                        Parameters = new Dictionary<string, string>
                        {
                            { Parameters.DirectoryPath, "." },
                            { Parameters.ExecutableArguments, "" },
                            { Parameters.ExecutableName, "PenPal.StitchA.exe" }
                        }
                    },
                };
                store.Save(packageFile, true);
                var stitch = new StitchInstance
                {
                    Name = "StitchA",
                    GroupName = groupName,
                    OwnerNodeName = core.Name,
                    OwnerNodeId = core.NodeId,
                    State = InstanceStateType.Running
                };
                store.Save(stitch, true);

                core.Start();

                Console.ReadKey();

                core.Stop();
            }
        }
    }
}
