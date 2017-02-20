using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Events;
using CrossStitch.Core.Modules.Stitches.Adaptors;
using CrossStitch.Core.Modules.Stitches.Messages;
using CrossStitch.Core.Utility.Networking;

namespace CrossStitch.Core.Modules.Stitches
{
    public class InstanceManager : IDisposable
    {
        private readonly StitchFileSystem _fileSystem;
        private readonly InstanceAdaptorFactory _adaptorFactory;
        private ConcurrentDictionary<string, IAppAdaptor> _adaptors;

        public InstanceManager(StitchFileSystem fileSystem, INetwork network)
        {
            _fileSystem = fileSystem;
            _adaptorFactory = new InstanceAdaptorFactory(network);
        }

        public event EventHandler<StitchStartedEventArgs> AppStarted;

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
                        return new InstanceActionResult
                        {
                            InstanceId = instanceId,
                            Success = false
                        };
                    }
                    adaptor.AppInitialized += AdaptorOnAppInitialized;
                }

                bool started = adaptor.Start();
                return new InstanceActionResult
                {
                    InstanceId = instance.Id,
                    Success = started
                };
            }
            catch (Exception e)
            {
                instance.State = InstanceStateType.Error;
                return new InstanceActionResult
                {
                    InstanceId = instance.Id,
                    Success = false,
                    Exception = e
                };
            }
        }

        public InstanceActionResult Stop(string instanceId)
        {
            try
            {
                IAppAdaptor adaptor;
                bool found = _adaptors.TryGetValue(instanceId, out adaptor);
                if (!found)
                {
                    return new InstanceActionResult
                    {
                        InstanceId = instanceId,
                        Success = false
                    };
                }
                adaptor.Stop();

                return new InstanceActionResult
                {
                    Success = true,
                    InstanceId = instanceId
                };
            }
            catch (Exception e)
            {
                return new InstanceActionResult
                {
                    Success = false,
                    Exception = e,
                    InstanceId = instanceId
                };
            }
        }

        public List<InstanceActionResult> StopAll()
        {
            var results = new List<InstanceActionResult>();

            foreach (var kvp in _adaptors)
            {
                var result = Stop(kvp.Key);
                results.Add(result);
            }

            return results;
        }

        //public InstanceActionResult CreateInstance(Instance instance)
        //{

        //    bool added = _instances.TryAdd(instance.Id, instance);
        //    if (!added)
        //        return InstanceActionResult.Failure();

        //    bool ok = _fileSystem.UnzipLibraryPackageToRunningBase(application.Name, component.Name, version.Version, instance.Id);
        //    if (!ok)
        //        return InstanceActionResult.Failure();

        //    return new InstanceActionResult
        //    {
        //        IsSuccess = true,
        //        InstanceId = instance.Id
        //    };
        //    return null;
        //}

        public InstanceActionResult RemoveInstance(string instanceId)
        {
            IAppAdaptor adaptor;
            bool removed = _adaptors.TryRemove(instanceId, out adaptor);
            if (removed)
                adaptor.Dispose();

            _fileSystem.DeleteRunningInstanceDirectory(instanceId);
            _fileSystem.DeleteDataInstanceDirectory(instanceId);
            return new InstanceActionResult
            {
                InstanceId = instanceId,
                Success = true
            };
        }

        public StitchResourceUsage GetInstanceResources(string instanceId)
        {
            IAppAdaptor adaptor;
            bool found = _adaptors.TryGetValue(instanceId, out adaptor);
            if (!found)
                return StitchResourceUsage.Empty();

            var usage = adaptor.GetResources();
            _fileSystem.GetInstanceDiskUsage(instanceId, usage);
            return usage;
        }

        public void Dispose()
        {
            StopAll();
            _adaptors.Clear();
            _adaptors = null;
        }

        private void AdaptorOnAppInitialized(object sender, StitchStartedEventArgs stitchStartedEventArgs)
        {
            //ComponentInstance instance;
            //bool found = _instances.TryGetValue(appStartedEventArgs.InstanceId, out instance);
            //if (!found)
            //    return;
            //instance.State = InstanceStateType.Running;
            AppStarted.Raise(this, stitchStartedEventArgs);
        }

        public List<InstanceInformation> GetInstanceInformation()
        {
            throw new NotImplementedException();
        }
    }
}