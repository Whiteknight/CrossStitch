using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Stitch.BuiltInClassV1;
using System;
using CrossStitch.Core.Utility;
using CrossStitch.Stitch;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.BuiltInClassV1
{
    public class BuiltInClassV1StitchAdaptor : IStitchAdaptor
    {
        private readonly StitchInstance _instance;
        private readonly IModuleLog _log;
        private readonly BuiltInClassV1Parameters _parameters;
        private object _stitchObject;

        public BuiltInClassV1StitchAdaptor(PackageFile packageFile, StitchInstance instance, IStitchEventObserver observer, IModuleLog log)
        {
            Observer = observer;
            _instance = instance;
            _log = log;
            _parameters = new BuiltInClassV1Parameters(packageFile.Adaptor.Parameters);
        }

        public IStitchEventObserver Observer { get; }

        public AdaptorType Type => AdaptorType.BuildInClassV1;

        public StitchResourceUsage GetResources()
        {
            return new StitchResourceUsage
            {
                ProcessId = 0,
            };
        }

        public void SendHeartbeat(long id)
        {
            try
            {
                var handles = _stitchObject as IHandlesHeartbeat;
                if (handles == null || handles.ReceiveHeartbeat(id))
                    Observer.StitchInstanceManagerOnHeartbeatReceived(_instance.Id, id);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Could not handle heartbeat");
            }
        }

        public void SendMessage(long messageId, string channel, string data, string nodeId, string senderStitchInstanceId)
        {
            try
            {
                var handles = _stitchObject as IHandlesMessages;
                if (handles == null)
                {
                    Observer.StitchInstanceManagerOnRequestResponseReceived(_instance.Id, messageId, false);
                    return;
                }
                bool ok = handles.ReceiveMessage(messageId, channel, data, nodeId, senderStitchInstanceId);
                Observer.StitchInstanceManagerOnRequestResponseReceived(_instance.Id, messageId, ok);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Could not send message");
            }
        }

        public bool Start()
        {
            try
            {
                _stitchObject = Activator.CreateInstance(_parameters.StitchType);
                var handlesStart = _stitchObject as IHandlesStart;
                return handlesStart == null || handlesStart.Start(Observer);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Could not start");
                return false;
            }
        }

        public void Stop()
        {
            try
            {
                if (_stitchObject == null)
                    return;

                var handlesStop = _stitchObject as IHandlesStop;
                handlesStop?.Stop();

                var disposable = _stitchObject as IDisposable;
                disposable?.Dispose();

                _stitchObject = null;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Could not stop");
            }
        }

        public void Dispose()
        {
            Stop();
            var disposable = _stitchObject as IDisposable;
            disposable?.Dispose();
        }
    }
}
