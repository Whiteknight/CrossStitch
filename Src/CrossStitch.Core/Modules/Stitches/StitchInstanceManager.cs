using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Stitches.Adaptors;
using CrossStitch.Stitch.Events;
using System;
using System.Collections.Generic;
using CrossStitch.Stitch.Process.Core;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchInstanceManager : IDisposable
    {
        private readonly StitchFileSystem _fileSystem;
        private readonly StitchAdaptorFactory _adaptorFactory;
        private readonly StitchAdaptorCollection _adaptors;

        public StitchInstanceManager(string nodeId, StitchesConfiguration configuration, StitchFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            // TODO: We need a way to get the unique string name of the node at this point.
            _adaptorFactory = new StitchAdaptorFactory(nodeId, configuration, _fileSystem);
            _adaptors = new StitchAdaptorCollection();
        }

        public event EventHandler<StitchProcessEventArgs> StitchStateChange;
        public event EventHandler<HeartbeatSyncReceivedEventArgs> HeartbeatReceived;
        public event EventHandler<RequestResponseReceivedEventArgs> RequestResponseReceived;
        public event EventHandler<LogsReceivedEventArgs> LogsReceived;
        public event EventHandler<DataMessageReceivedEventArgs> DataMessageReceived;

        public InstanceActionResult Start(PackageFile packageFile, StitchInstance stitchInstance)
        {
            if (string.IsNullOrEmpty(stitchInstance?.Id))
                return InstanceActionResult.BadRequest();

            string instanceId = stitchInstance.Id;
            IStitchAdaptor adaptor = null;

            try
            {
                stitchInstance.State = InstanceStateType.Stopped;
                adaptor = GetOrCreateStitchAdaptor(packageFile, stitchInstance);
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
                return InstanceActionResult.Failure(stitchInstance.Id, adaptor != null, e);
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
                stitchInstance.State = InstanceStateType.Stopped;

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
            adaptor.StitchContext.DataDirectory = _fileSystem.GetInstanceDataDirectoryPath(stitchInstance.Id);
            adaptor.StitchContext.StitchStateChange += OnStitchStateChange;
            adaptor.StitchContext.HeartbeatReceived += OnStitchHeartbeatSyncReceived;
            adaptor.StitchContext.LogsReceived += OnStitchLogsReceived;
            adaptor.StitchContext.RequestResponseReceived += OnStitchResponseReceived;
            adaptor.StitchContext.DataMessageReceived += OnDataMessageReceived;

            _adaptors.Add(stitchInstance.Id, adaptor);
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