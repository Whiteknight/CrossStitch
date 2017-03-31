using System;
using CrossStitch.Backplane.Zyre;
using CrossStitch.Core;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;

namespace StitchIntegration.ServerB
{
    class Program
    {
        static void Main(string[] args)
        {
            var nodeConfig = NodeConfiguration.GetDefault();
            nodeConfig.NodeId = "ServerB";
            nodeConfig.NodeName = "StitchIntegration.ServerB";
            using (var core = new CrossStitchCore(nodeConfig))
            {
                Console.Title = core.Name;
                core.AddModule(new BackplaneModule(core));
                core.AddModule(new StitchesModule(core));
                core.AddModule(new LoggingModule(core, Common.Logging.LogManager.GetLogger("CrossStitch")));

                // This Node just runs. ServerA will generate and send commands to this node to produce changes
                core.Start();

                Console.ReadKey();

                core.Stop();
            }
        }
    }
}
