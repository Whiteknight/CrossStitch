using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Modules;
using CrossStitch.Core.Modules.Data;
using CrossStitch.Core.Modules.Data.InMemory;
using CrossStitch.Core.Modules.Master;
using CrossStitch.Core.Modules.StitchMonitor;
using CrossStitch.Core.Modules.Timer;
using System;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Stitch.Utility.Extensions;

namespace CrossStitch.Core
{
    public class ModuleCollection
    {
        private readonly Dictionary<string, IModule> _modules;
        private readonly List<string> _autocreatedModules;

        private static readonly Dictionary<string, int> _priorities = new Dictionary<string, int>
        {
            { ModuleNames.Log, 1 },
            { ModuleNames.Data, 2 },
            { ModuleNames.Master, 3 },
            { ModuleNames.Stitches, 98 },
            { ModuleNames.StitchMonitor, 99 }
        };

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
            return _modules.GetOrDefault(name);
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
            if (!_modules.ContainsKey(ModuleNames.StitchMonitor))
                Add(new StitchMonitorModule(core, core.Configuration));
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

        public void StartAll()
        {
            var modules = _modules.Values.OrderBy(m => _priorities.GetOrDefault(m.Name, 100)).ToList();

            foreach (var module in modules)
                module.Start();
        }

        public void StopAll(CrossStitchCore core)
        {
            foreach (var module in _modules.Values)
                module.Stop();
        }

        public IEnumerable<string> AddedModules => _modules.Keys;
    }
}