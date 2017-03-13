using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Stitches.Adaptors;
using CrossStitch.Stitch.Events;
using CrossStitch.Stitch.V1.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchInstanceManager : IDisposable
    {
        private readonly StitchFileSystem _fileSystem;
        private readonly StitchAdaptorFactory _adaptorFactory;
        private ConcurrentDictionary<string, IStitchAdaptor> _adaptors;

        public StitchInstanceManager(StitchFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            // TODO: We need a way to get the unique string name of the node at this point.
            _adaptorFactory = new StitchAdaptorFactory();
            _adaptors = new ConcurrentDictionary<string, IStitchAdaptor>();
        }

        public event EventHandler<StitchProcessEventArgs> StitchStateChange;
        public event EventHandler<HeartbeatSyncReceivedEventArgs> HeartbeatReceived;
        public event EventHandler<RequestResponseReceivedEventArgs> RequestResponseReceived;
        public event EventHandler<LogsReceivedEventArgs> LogsReceived;
        public event EventHandler<DataMessageReceivedEventArgs> DataMessageReceived;

        public InstanceActionResult Start(StitchInstance stitchInstance)
        {
            string instanceId = stitchInstance.Id;
            IStitchAdaptor adaptor = null;

            try
            {
                stitchInstance.State = InstanceStateType.Stopped;
                adaptor = GetStitchAdaptor(stitchInstance);
                if (adaptor == null)
                    return InstanceActionResult.NotFound(instanceId);

                // TODO: On Stitch start, we should send it information about the application topology
                // TODO: We should also send application topology change notifications to every Stitch 
                // involved in the affected application.

                bool started = adaptor.Start();
                if (started)
                    stitchInstance.State = InstanceStateType.Started;

                return InstanceActionResult.Result(instanceId, started);
            }
            catch (Exception e)
            {
                stitchInstance.State = InstanceStateType.Error;
                return InstanceActionResult.Failure(stitchInstance.Id, adaptor != null, e);
            }
        }

        private IStitchAdaptor GetStitchAdaptor(StitchInstance stitchInstance)
        {
            IStitchAdaptor adaptor;
            bool found = _adaptors.TryGetValue(stitchInstance.Id, out adaptor);
            if (found)
                return adaptor;

            adaptor = CreateStitchAdaptor(stitchInstance);
            if (adaptor != null)
                return adaptor;

            stitchInstance.State = InstanceStateType.Missing;
            return null;
        }

        private IStitchAdaptor CreateStitchAdaptor(StitchInstance stitchInstance)
        {
            var adaptor = _adaptorFactory.Create(stitchInstance);
            bool added = _adaptors.TryAdd(stitchInstance.Id, adaptor);
            if (!added)
                return null;

            adaptor.StitchContext.StitchStateChange += OnStitchStateChange;
            adaptor.StitchContext.HeartbeatReceived += OnStitchHeartbeatSyncReceived;
            adaptor.StitchContext.LogsReceived += OnStitchLogsReceived;
            adaptor.StitchContext.RequestResponseReceived += OnStitchResponseReceived;
            adaptor.StitchContext.DataMessageReceived += OnDataMessageReceived;
            return adaptor;
        }

        public InstanceActionResult Stop(string instanceId)
        {
            IStitchAdaptor adaptor = null;
            try
            {
                bool found = _adaptors.TryGetValue(instanceId, out adaptor);
                if (!found)
                    return InstanceActionResult.NotFound(instanceId);
                adaptor.Stop();

                return InstanceActionResult.Result(instanceId, true);
            }
            catch (Exception e)
            {
                return InstanceActionResult.Failure(instanceId, adaptor != null, e);
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
            return InstanceActionResult.Result(instanceId, true);
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

        public List<InstanceInformation> GetInstanceInformation()
        {
            throw new NotImplementedException();
        }

        public InstanceActionResult SendHeartbeat(long heartbeatId, StitchInstance instance)
        {
            IStitchAdaptor adaptor;
            bool found = _adaptors.TryGetValue(instance.Id, out adaptor);
            if (!found)
                return InstanceActionResult.NotFound(instance.Id);

            adaptor.SendHeartbeat(heartbeatId);
            return InstanceActionResult.Result(instance.Id, true, instance);
        }

        public InstanceActionResult SendDataMessage(StitchDataMessage message)
        {
            IStitchAdaptor adaptor;
            bool found = _adaptors.TryGetValue(message.ToStitchInstanceId, out adaptor);
            if (!found)
                return InstanceActionResult.NotFound(message.ToStitchInstanceId);

            adaptor.SendMessage(message.Id, message.DataChannelName, message.Data, message.FromNodeId, message.FromStitchInstanceId);
            return InstanceActionResult.Result(message.ToStitchInstanceId, true);
        }

        private void OnStitchStateChange(object sender, StitchProcessEventArgs stitchProcessEventArgs)
        {
            StitchStateChange.Raise(this, stitchProcessEventArgs);
        }

        private void OnStitchLogsReceived(object sender, LogsReceivedEventArgs e)
        {
            LogsReceived.Raise(this, e);
        }

        private void OnStitchHeartbeatSyncReceived(object sender, HeartbeatSyncReceivedEventArgs e)
        {
            HeartbeatReceived.Raise(this, e);
        }

        private void OnStitchResponseReceived(object sender, RequestResponseReceivedEventArgs e)
        {
            RequestResponseReceived.Raise(this, e);
        }

        private void OnDataMessageReceived(object sender, DataMessageReceivedEventArgs e)
        {
            DataMessageReceived.Raise(this, e);
        }
    }
}