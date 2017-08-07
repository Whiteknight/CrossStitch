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
        private readonly IModuleLog _log;
        private readonly BuiltInClassV1Parameters _parameters;
        private object _stitchObject;

        public BuiltInClassV1StitchAdaptor(PackageFile packageFile, CoreStitchContext stitchContext, IModuleLog log)
        {
            StitchContext = stitchContext;
            _log = log;
            _parameters = new BuiltInClassV1Parameters(packageFile.Adaptor.Parameters);
        }

        public CoreStitchContext StitchContext { get; }

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
                    StitchContext.ReceiveHeartbeatSync(id);
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
                    StitchContext.ReceiveResponse(messageId, false);
                    return;
                }
                bool ok = handles.ReceiveMessage(messageId, channel, data, nodeId, senderStitchInstanceId);
                StitchContext.ReceiveResponse(messageId, ok);
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
                return handlesStart == null || handlesStart.Start(StitchContext);
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
