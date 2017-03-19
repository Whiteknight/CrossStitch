using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Stitches.Adaptors;
using CrossStitch.Stitch.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CrossStitch.Stitch.ProcessV1.Core;

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
            if (string.IsNullOrEmpty(stitchInstance?.Id))
                return InstanceActionResult.BadRequest();

            string instanceId = stitchInstance.Id;
            IStitchAdaptor adaptor = null;

            try
            {
                stitchInstance.State = InstanceStateType.Stopped;
                adaptor = GetOrCreateStitchAdaptor(stitchInstance);
                if (adaptor == null)
                    return InstanceActionResult.NotFound(instanceId);

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
                return InstanceActionResult.Failure(stitchInstance.Id, adaptor != null, e);
            }
        }

        public InstanceActionResult Stop(StitchInstance stitchInstance)
        {
            IStitchAdaptor adaptor = null;
            try
            {
                bool found = _adaptors.TryGetValue(stitchInstance.Id, out adaptor);
                if (!found)
                    return InstanceActionResult.NotFound(stitchInstance.Id);
                adaptor.Stop();
                stitchInstance.State = InstanceStateType.Stopped;

                return InstanceActionResult.Result(stitchInstance.Id, true, stitchInstance);
            }
            catch (Exception e)
            {
                return InstanceActionResult.Failure(stitchInstance.Id, adaptor != null, stitchInstance, e);
            }
        }

        public List<InstanceActionResult> StopAll()
        {
            var results = new List<InstanceActionResult>();

            foreach (var kvp in _adaptors)
            {
                try
                {
                    var adaptor = kvp.Value;
                    adaptor.Stop();
                    results.Add(new InstanceActionResult
                    {
                        Found = true,
                        Success = true,
                        InstanceId = kvp.Key
                    });
                }
                catch (Exception e)
                {
                    results.Add(new InstanceActionResult
                    {
                        Found = true,
                        Success = false,
                        InstanceId = kvp.Key,
                        Exception = e
                    });
                }
            }

            return results;
        }

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

        public List<InstanceInformation> GetInstanceInformation()
        {
            throw new NotImplementedException();
        }

        public void SendHeartbeat(long heartbeatId)
        {
            foreach (var adaptor in _adaptors.Values.ToList())
            {
                adaptor.SendHeartbeat(heartbeatId);
            }
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

        public int GetNumberOfRunningStitches()
        {
            return _adaptors.Count;
        }

        public void Dispose()
        {
            StopAll();
            _adaptors.Clear();
            _adaptors = null;
        }

        private IStitchAdaptor GetOrCreateStitchAdaptor(StitchInstance stitchInstance)
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