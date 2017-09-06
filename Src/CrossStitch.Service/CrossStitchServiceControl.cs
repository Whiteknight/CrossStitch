using CrossStitch.Backplane.Zyre;
using CrossStitch.Core;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.Folders;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
using CrossStitch.Http.NancyFx;
using Microsoft.Extensions.Logging;
using Topshelf;

namespace CrossStitch.Service
{
    public class CrossStitchServiceControl : ServiceControl
    {
        private readonly CrossStitchCore _core;

        public CrossStitchServiceControl()
        {
            var nodeConfiguration = NodeConfiguration.GetDefault();
            _core = new CrossStitchCore(nodeConfiguration);
            var logger = new LoggerFactory().AddConsole(LogLevel.Debug).CreateLogger<Program>();
            _core.AddModule(new LoggingModule(_core, logger));
            _core.AddModule(new NancyHttpModule(_core.MessageBus));
            _core.AddModule(new DataModule(_core.MessageBus, new FolderDataStorage()));
            _core.AddModule(new StitchesModule(_core));
            _core.AddModule(new BackplaneModule(_core));
        }

        public bool Start(HostControl hostControl)
        {
            _core.Start();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _core.Stop();
            return true;
        }
    }
}
