using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Modules;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.InMemory;
using CrossStitch.Core.Modules.Master;
using CrossStitch.Core.Modules.RequestCoordinator;
using CrossStitch.Core.Modules.StitchMonitor;
using CrossStitch.Core.Modules.Timer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core
{
    public class ModuleCollection
    {
        private readonly Dictionary<string, IModule> _modules;
        private readonly List<string> _autocreatedModules;

        public ModuleCollection()
        {
            _modules = new Dictionary<string, IModule>();
            _autocreatedModules = new List<string>();
        }

        public void Add(IModule module)
        {
            if (_modules.ContainsKey(module.Name))
                throw new Exception("Cannot have multiple modules of the same name");

            _modules.Add(module.Name, module);
        }

        public IModule Get(string name)
        {
            if (!_modules.ContainsKey(name))
                return null;
            return _modules[name];
        }

        private void AddWithWarning(IModule module)
        {
            Add(module);
            _autocreatedModules.Add(module.Name);
        }

        public void AddMissingModules(CrossStitchCore core)
        {
            // These core modules are almost always auto-created and don't require warnings
            if (!_modules.ContainsKey(ModuleNames.Timer))
                Add(new MessageTimerModule(core.MessageBus));
            if (!_modules.ContainsKey(ModuleNames.RequestCoordinator))
                Add(new RequestCoordinatorModule());
            if (!_modules.ContainsKey(ModuleNames.StitchMonitor))
                Add(new StitchMonitorModule(core.Configuration));
            if (!_modules.ContainsKey(ModuleNames.Master))
                Add(new MasterModule(core, core.Configuration));

            // These modules are necessary for basic operation, but defaulting is not
            // straight-forward, so we need to raise a warning.
            if (!_modules.ContainsKey(ModuleNames.Data))
                AddWithWarning(new DataModule(core.MessageBus, new InMemoryDataStorage()));
        }

        public void WarnOnMissingModules(ModuleLog log)
        {
            foreach (var autoModule in _autocreatedModules)
                log.LogWarning("Module {0} was not specified, so it will be automatically created. You may be missing features or configurations if the wrong implementation was chosen.", autoModule);

            if (!_modules.ContainsKey(ModuleNames.Stitches))
                log.LogWarning("Stitches module was not added. This CrossStitch instance will not be able to host Stitches");
            if (!_modules.ContainsKey(ModuleNames.Backplane))
                log.LogWarning("Backplane module was not added. This CrossStitch instance will not be able to join a cluster with other nodes.");
        }

        public void StartAll(CrossStitchCore core)
        {
            // Start the "Log" module first, to make sure that messages generated during startup
            // for other modules are logged.
            // TODO: We should have some kind of mechanism to tag a module with a priority, so we
            // can start high-priority modules first.
            // TODO: We definitely want the DataModule to initialize early, in case other modules
            // need to get stored config/state data
            foreach (var module in _modules.Values.Where(m => m.Name == ModuleNames.Log))
                module.Start(core);
            foreach (var module in _modules.Values.Where(m => m.Name != ModuleNames.Log))
                module.Start(core);
        }

        public void StopAll(CrossStitchCore core)
        {
            foreach (var module in _modules.Values)
                module.Stop();
        }

        public IEnumerable<string> AddedModules => _modules.Keys;
    }
}