﻿using CrossStitch.App.Events;
using CrossStitch.App.Networking;
using CrossStitch.Core.Apps.Messages;
using CrossStitch.Core.Data.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Apps
{
    public class InstanceManager : IDisposable
    {
        private readonly AppsConfiguration _config;
        private readonly AppFileSystem _fileSystem;
        private readonly AppsDataStorage _storage;
        private readonly InstanceAdaptorFactory _adaptorFactory;
        private ConcurrentDictionary<string, IAppAdaptor> _adaptors;

        public InstanceManager(AppsConfiguration config, AppFileSystem fileSystem, AppsDataStorage storage, INetwork network)
        {
            _config = config;
            _fileSystem = fileSystem;
            _storage = storage;
            _adaptorFactory = new InstanceAdaptorFactory(network);
        }

        public event EventHandler<AppStartedEventArgs> AppStarted;

        public List<InstanceActionResult> StartupActiveInstances(IEnumerable<Instance> instances)
        {
            if (_adaptors != null)
                throw new Exception("InstanceManager already started");

            _adaptors = new ConcurrentDictionary<string, IAppAdaptor>();

            List<InstanceActionResult> results = new List<InstanceActionResult>();
            foreach (var instance in instances.Where(i => i.State == InstanceStateType.Running))
            {
                var result = Start(instance);
                results.Add(result);
            }
            
            return results;
        }

        public InstanceActionResult Start(Instance instance)
        {
            string instanceId = instance.Id;

            try
            {
                instance.State = InstanceStateType.Started;

                IAppAdaptor adaptor;
                bool found = _adaptors.TryGetValue(instanceId, out adaptor);
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
                return new InstanceActionResult
                {
                    InstanceId = instance.Id,
                    IsSuccess = false,
                    Exception = e
                };
            }
        }

        public InstanceActionResult Stop(string instanceId, bool persistState)
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
                adaptor.Stop();

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

        public InstanceActionResult CreateInstance(ClientApplication application, ApplicationComponent component, ComponentVersion version)
        {
            //var instance = new ComponentInstance(Guid.NewGuid(), version.Id) { State = InstanceStateType.Stopped };
            //bool added = _instances.TryAdd(instance.Id, instance);
            //if (!added)
            //    return InstanceActionResult.Failure();

            //bool ok = _fileSystem.UnzipLibraryPackageToRunningBase(application.Name, component.Name, version.Version, instance.Id);
            //if (!ok)
            //    return InstanceActionResult.Failure();

            //return new InstanceActionResult {
            //    IsSuccess = true,
            //    InstanceId = instance.Id
            //};
            return null;
        }

        public InstanceActionResult RemoveInstance(string instanceId)
        {
            IAppAdaptor adaptor;
            bool removed = _adaptors.TryRemove(instanceId, out adaptor);
            if (removed)
                adaptor.Dispose();

            _fileSystem.DeleteRunningInstanceDirectory(instanceId);
            _fileSystem.DeleteDataInstanceDirectory(instanceId);
            return new InstanceActionResult {
                InstanceId = instanceId,
                IsSuccess = true
            };
        }

        public AppResourceUsage GetInstanceResources(string instanceId)
        {
            IAppAdaptor adaptor;
            bool found = _adaptors.TryGetValue(instanceId, out adaptor);
            if (!found)
                return AppResourceUsage.Empty();

            var usage = adaptor.GetResources();
            _fileSystem.GetInstanceDiskUsage(instanceId, usage);
            return usage;
        }

        public void Dispose()
        {
            StopAll(false);
            _adaptors.Clear();
            _adaptors = null;
        }

        private void AdaptorOnAppInitialized(object sender, AppStartedEventArgs appStartedEventArgs)
        {
            //ComponentInstance instance;
            //bool found = _instances.TryGetValue(appStartedEventArgs.InstanceId, out instance);
            //if (!found)
            //    return;
            //instance.State = InstanceStateType.Running;
            AppStarted.Raise(this, appStartedEventArgs);
        }

        public List<InstanceInformation> GetInstanceInformation()
        {
            throw new NotImplementedException();
        }
    }
}