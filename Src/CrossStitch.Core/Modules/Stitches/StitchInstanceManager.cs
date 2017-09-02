using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Stitches.Adaptors;
using System;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchInstanceManager : IDisposable
    {
        private readonly StitchFileSystem _fileSystem;
        private readonly StitchAdaptorFactory _adaptorFactory;
        private readonly StitchAdaptorCollection _adaptors;

        public StitchInstanceManager(StitchFileSystem fileSystem, StitchAdaptorFactory adaptorFactory)
        {
            _fileSystem = fileSystem;
            
            // TODO: We need a way to get the unique string name of the node at this point.
            _adaptorFactory = adaptorFactory;
            _adaptors = new StitchAdaptorCollection();
        }

        public InstanceActionResult Start(PackageFile packageFile, StitchInstance stitchInstance)
        {
            if (stitchInstance == null || string.IsNullOrEmpty(stitchInstance.Id))
                return InstanceActionResult.BadRequest();

            stitchInstance.State = InstanceStateType.Stopped;
            string instanceId = stitchInstance.Id;

            try
            {
                var adaptor = GetOrCreateStitchAdaptor(packageFile, stitchInstance);
                if (adaptor == null)
                {
                    stitchInstance.State = InstanceStateType.Missing;
                    return InstanceActionResult.NotFound(instanceId);
                }

                // TODO: On Stitch start, we should send it information about the application topology
                // TODO: We should also send application topology change notifications to every Stitch 
                // involved in the affected application.

                bool started = adaptor.Start();
                if (started)
                    stitchInstance.State = InstanceStateType.Started;

                return InstanceActionResult.Result(instanceId, started, stitchInstance);
            }
            catch (Exception e)
            {
                stitchInstance.State = InstanceStateType.Error;
                return InstanceActionResult.Failure(stitchInstance.Id, true, e);
            }
        }

        public InstanceActionResult Stop(StitchInstance stitchInstance)
        {
            try
            {
                var adaptor = _adaptors.Get(stitchInstance.Id);
                if (adaptor == null)
                    return InstanceActionResult.NotFound(stitchInstance.Id);

                adaptor.Stop();
                stitchInstance.State = InstanceStateType.Stopping;

                return InstanceActionResult.Result(stitchInstance.Id, true, stitchInstance);
            }
            catch (Exception e)
            {
                return InstanceActionResult.Failure(stitchInstance.Id, true, stitchInstance, e);
            }
        }

        public List<InstanceActionResult> StopAll()
        {
            var results = new List<InstanceActionResult>();

            _adaptors.ForEach((id, adaptor) =>
            {
                try
                {
                    adaptor.Stop();
                    results.Add(new InstanceActionResult
                    {
                        Found = true,
                        Success = true,
                        InstanceId = id
                    });
                }
                catch (Exception e)
                {
                    results.Add(new InstanceActionResult
                    {
                        Found = true,
                        Success = false,
                        InstanceId = id,
                        Exception = e
                    });
                }
            });

            return results;
        }

        public InstanceActionResult RemoveInstance(string instanceId)
        {
            var adaptor = _adaptors.Remove(instanceId);
            adaptor?.Dispose();

            _fileSystem.DeleteRunningInstanceDirectory(instanceId);
            _fileSystem.DeleteDataInstanceDirectory(instanceId);
            return InstanceActionResult.Result(instanceId, adaptor != null);
        }

        public StitchResourceUsage GetInstanceResources(string instanceId)
        {
            var adaptor = _adaptors.Get(instanceId);
            if (adaptor == null)
                return StitchResourceUsage.Empty();

            var usage = adaptor.GetResources();
            _fileSystem.GetInstanceDiskUsage(instanceId, usage);
            return usage;
        }

        public List<InstanceInformation> GetInstanceInformation()
        {
            throw new NotImplementedException();
        }

        public void SendHeartbeat(long heartbeatId)
        {
            _adaptors.ForEach((id, adaptor) =>
            {
                adaptor.SendHeartbeat(heartbeatId);
            });
        }

        public InstanceActionResult SendDataMessage(StitchFullId fullStitchId, StitchDataMessage message)
        {
            var adaptor = _adaptors.Get(fullStitchId.StitchInstanceId);
            if (adaptor == null)
                return InstanceActionResult.NotFound(fullStitchId.StitchInstanceId);

            adaptor.SendMessage(message.Id, message.DataChannelName, message.Data, message.FromNodeId, message.FromStitchInstanceId);
            return InstanceActionResult.Result(fullStitchId.StitchInstanceId, true);
        }

        public int GetNumberOfRunningStitches()
        {
            return _adaptors.Count;
        }

        public void Dispose()
        {
            StopAll();
            _adaptors.Clear();
        }

        private IStitchAdaptor GetOrCreateStitchAdaptor(PackageFile packageFile, StitchInstance stitchInstance)
        {
            var adaptor = _adaptors.Get(stitchInstance.Id);
            if (adaptor != null)
                return adaptor;

            adaptor = _adaptorFactory.Create(packageFile, stitchInstance);

            _adaptors.Add(stitchInstance.Id, adaptor);
            return adaptor;
        }
    }
}