using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Events;
using CrossStitch.Core.Modules.Stitches.Adaptors;
using CrossStitch.Core.Modules.Stitches.Messages;
using CrossStitch.Stitch;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchInstanceManager : IDisposable
    {
        private readonly StitchFileSystem _fileSystem;
        private readonly StitchAdaptorFactory _adaptorFactory;
        private ConcurrentDictionary<string, IStitchAdaptor> _adaptors;

        public StitchInstanceManager(IRunningNodeContext nodeContext, StitchFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            // TODO: We need a way to get the unique string name of the node at this point.
            _adaptorFactory = new StitchAdaptorFactory(nodeContext);
        }

        public event EventHandler<StitchProcessEventArgs> StitchStarted;

        public List<InstanceActionResult> StartupActiveInstances(IEnumerable<StitchInstance> instances)
        {
            if (_adaptors != null)
                throw new Exception("InstanceManager already started");

            _adaptors = new ConcurrentDictionary<string, IStitchAdaptor>();

            var results = new List<InstanceActionResult>();
            foreach (var instance in instances.Where(i => i.State == InstanceStateType.Running || i.State == InstanceStateType.Started))
            {
                var result = Start(instance);
                results.Add(result);
            }

            return results;
        }

        public InstanceActionResult Start(StitchInstance stitchInstance)
        {
            string instanceId = stitchInstance.Id;
            IStitchAdaptor adaptor = null;

            try
            {
                stitchInstance.State = InstanceStateType.Started;


                bool found = _adaptors.TryGetValue(instanceId, out adaptor);
                if (!found)
                {
                    adaptor = _adaptorFactory.Create(stitchInstance);
                    bool added = _adaptors.TryAdd(instanceId, adaptor);
                    if (!added)
                    {
                        return new InstanceActionResult
                        {
                            InstanceId = instanceId,
                            Success = false,
                            Found = false
                        };
                    }
                    adaptor.StitchInitialized += AdaptorOnStitchInitialized;
                }

                bool started = adaptor.Start();
                return new InstanceActionResult
                {
                    InstanceId = stitchInstance.Id,
                    Success = started,
                    Found = true
                };
            }
            catch (Exception e)
            {
                stitchInstance.State = InstanceStateType.Error;
                return new InstanceActionResult
                {
                    InstanceId = stitchInstance.Id,
                    Success = false,
                    Exception = e,
                    Found = adaptor != null
                };
            }
        }

        public InstanceActionResult Stop(string instanceId)
        {
            IStitchAdaptor adaptor = null;
            try
            {
                bool found = _adaptors.TryGetValue(instanceId, out adaptor);
                if (!found)
                {
                    return new InstanceActionResult
                    {
                        InstanceId = instanceId,
                        Success = false,
                        Found = false
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
                    Found = adaptor != null,
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
            IStitchAdaptor adaptor;
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
            IStitchAdaptor adaptor;
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

        private void AdaptorOnStitchInitialized(object sender, StitchProcessEventArgs stitchProcessEventArgs)
        {
            //ComponentInstance instance;
            //bool found = _instances.TryGetValue(appStartedEventArgs.InstanceId, out instance);
            //if (!found)
            //    return;
            //instance.State = InstanceStateType.Running;
            StitchStarted.Raise(this, stitchProcessEventArgs);
        }

        public List<InstanceInformation> GetInstanceInformation()
        {
            throw new NotImplementedException();
        }

        public List<InstanceActionResult> SendHeartbeats(long id, IEnumerable<StitchInstance> instances)
        {
            var results = new List<InstanceActionResult>();
            foreach (var instance in instances)
            {
                IStitchAdaptor adaptor;
                bool found = _adaptors.TryGetValue(instance.Id, out adaptor);
                if (!found)
                {
                    results.Add(new InstanceActionResult
                    {
                        Found = false,
                        InstanceId = instance.Id,
                        Success = false
                    });
                    continue;
                }

                bool ok = adaptor.SendHeartbeat(id);
                results.Add(new InstanceActionResult
                {
                    StitchInstance = instance,
                    InstanceId = instance.Id,
                    Found = true,
                    Success = ok
                });
            }
            return results;
        }
    }
}