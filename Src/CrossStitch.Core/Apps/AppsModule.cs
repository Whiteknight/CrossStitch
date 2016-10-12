using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Apps.Events;
using CrossStitch.Core.Configuration;
using CrossStitch.Core.Logging.Events;
using CrossStitch.Core.Messaging;

namespace CrossStitch.Core.Apps
{
    public class AppsConfiguration
    {
        public static AppsConfiguration GetDefault()
        {
            return ConfigurationLoader.GetConfiguration<AppsConfiguration>("apps.json");
        }

        public void SetDefaults()
        {
            
        }

        public string DataBasePath { get; set; }
        public string AppLibraryBasePath { get; set; }
        public string RunningAppBasePath { get; set; }
    }

    public class AppsModule : IModule
    {
        private readonly AppsConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        private readonly InstanceManager _instances;
        private readonly AppDataStorage _dataStorage;

        public AppsModule(AppsConfiguration configuration, IMessageBus messageBus)
        {
            _configuration = configuration;
            _messageBus = messageBus;
            _instances = new InstanceManager(configuration, 
                new AppFileSystem(configuration),
                new AppDataStorage());
        }

        public string Name { get { return "Apps"; } }
        public void Start(RunningNode context)
        {
            var results = _instances.StartupActiveInstances();
            foreach (var result in results.Where(isr => isr.IsSuccess == false))
            {
                _messageBus.Publish(LogEvent.Error, new LogEvent {
                    Exception = result.Exception,
                    Message = "Instance " + result.InstanceId + " failed to start"
                });
            }
            foreach (var result in results.Where(isr => isr.IsSuccess == true))
            {
                _messageBus.Publish(AppInstanceEvent.StartedEventName, new AppInstanceEvent {
                    InstanceId = result.InstanceId,
                    NodeId = context.NodeId
                });
            }
        }

        public void Stop()
        {
            _instances.StopAll(false);
        }

        public void Dispose()
        {
            Stop();
            _instances.Dispose();
        }
    }

    public class AppDataStorage
    {
        public List<ClientApplication> GetAllApplications()
        {
            return null;
        }

        public List<ComponentInstance> GetAllInstances()
        {
            return null;
        }

        public void Save(ComponentInstance instance)
        {
        }
    }

    public class InstanceActionResult
    {
        public Guid InstanceId { get; set; }
        public bool IsSuccess { get; set; }
        public Exception Exception { get; set; }
    }

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
    }
}
