using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.App.Events;

namespace CrossStitch.Core.Apps
{
    public class InstanceManager : IDisposable
    {
        private readonly AppsConfiguration _config;
        private readonly AppFileSystem _fileSystem;
        private readonly AppDataStorage _storage;
        private ConcurrentDictionary<Guid, ComponentInstance> _instances;
        private readonly InstanceAdaptorFactory _adaptorFactory;
        private ConcurrentDictionary<Guid, IAppAdaptor> _adaptors;

        public InstanceManager(AppsConfiguration config, AppFileSystem fileSystem, AppDataStorage storage)
        {
            _config = config;
            _fileSystem = fileSystem;
            _storage = storage;
            _adaptorFactory = new InstanceAdaptorFactory();
        }

        public event EventHandler<AppStartedEventArgs> AppStarted;

        public List<InstanceActionResult> StartupActiveInstances()
        {
            if (_instances != null)
                throw new Exception("InstanceManager already started");

            var instances = _storage.GetAllInstances().ToDictionary(i => i.Id);
            _instances = new ConcurrentDictionary<Guid, ComponentInstance>(instances);
            _adaptors = new ConcurrentDictionary<Guid, IAppAdaptor>();

            List<InstanceActionResult> results = new List<InstanceActionResult>();
            foreach (var instance in instances.Values.Where(i => i.State == InstanceStateType.Running))
            {
                var result = Start(instance.Id);
                results.Add(result);
            }

            return results;
        }

        public InstanceActionResult Start(Guid instanceId)
        {
            ComponentInstance instance;
            bool found = _instances.TryGetValue(instanceId, out instance);
            if (!found)
            {
                return new InstanceActionResult {
                    InstanceId = instanceId,
                    IsSuccess = false
                };
            }
            try
            {
                instance.State = InstanceStateType.Started;
                IAppAdaptor adaptor;
                found = _adaptors.TryGetValue(instanceId, out adaptor);
                if (!found)
                {
                    adaptor = _adaptorFactory.Create(instance);
                    bool added = _adaptors.TryAdd(instanceId, adaptor);
                    if (!added)
                    {
                        return new InstanceActionResult {
                            InstanceId = instanceId,
                            IsSuccess = false
                        };
                    }
                    adaptor.AppInitialized += AdaptorOnAppInitialized;
                }
                
                bool started = adaptor.Start();
                return new InstanceActionResult
                {
                    InstanceId = instance.Id,
                    IsSuccess = started
                };
            }
            catch (Exception e)
            {
                instance.State = InstanceStateType.Error;
                _storage.Save(instance);
                return new InstanceActionResult
                {
                    InstanceId = instance.Id,
                    IsSuccess = false,
                    Exception = e
                };
            }
        }

        public InstanceActionResult Stop(Guid instanceId, bool persistState)
        {
            try
            {
                IAppAdaptor adaptor;
                bool found = _adaptors.TryGetValue(instanceId, out adaptor);
                if (!found)
                {
                    return new InstanceActionResult {
                        InstanceId = instanceId,
                        IsSuccess = false
                    };
                }
                if (persistState)
                {
                    ComponentInstance instance;
                    found = _instances.TryGetValue(instanceId, out instance);
                    if (found)
                    {
                        instance.State = InstanceStateType.Stopped;
                        _storage.Save(instance);
                    }
                }
                return new InstanceActionResult
                {
                    IsSuccess = true,
                    InstanceId = instanceId
                };
            }
            catch (Exception e)
            {
                return new InstanceActionResult
                {
                    IsSuccess = false,
                    Exception = e,
                    InstanceId = instanceId
                };
            }
        }

        public List<InstanceActionResult> StopAll(bool persistState)
        {
            var results = new List<InstanceActionResult>();

            foreach (var kvp in _adaptors)
            {
                var result = Stop(kvp.Key, persistState);
                results.Add(result);
            }

            return results;
        }

        public void Dispose()
        {
            StopAll(false);
            _instances.Clear();
            _instances = null;
            _adaptors.Clear();
            _adaptors = null;
        }

        private void AdaptorOnAppInitialized(object sender, AppStartedEventArgs appStartedEventArgs)
        {
            ComponentInstance instance;
            bool found = _instances.TryGetValue(appStartedEventArgs.InstanceId, out instance);
            if (!found)
                return;
            instance.State = InstanceStateType.Running;
            AppStarted.Raise(this, appStartedEventArgs);
        }
    }
}