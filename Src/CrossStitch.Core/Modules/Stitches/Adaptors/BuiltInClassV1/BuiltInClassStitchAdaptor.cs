using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Stitch.BuiltInClassV1;
using CrossStitch.Stitch.ProcessV1.Core;
using System;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.BuiltInClassV1
{
    public class BuiltInClassV1StitchAdaptor : IStitchAdaptor
    {
        private readonly StitchInstance _stitchInstance;
        private readonly BuiltInClassV1Parameters _parameters;
        private object _stitchObject;

        public BuiltInClassV1StitchAdaptor(StitchInstance stitchInstance, CoreStitchContext stitchContext)
        {
            StitchContext = stitchContext;
            _stitchInstance = stitchInstance;
            _parameters = new BuiltInClassV1Parameters(stitchInstance.Adaptor.Parameters);
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
            var handles = _stitchObject as IHandlesHeartbeat;
            if (handles == null)
                StitchContext.ReceiveHeartbeatSync(id);
            else
            {
                bool ok = handles.ReceiveHeartbeat(id);
                if (ok)
                    StitchContext.ReceiveHeartbeatSync(id);
            }
        }

        public void SendMessage(long messageId, string channel, string data, string nodeId, string senderStitchInstanceId)
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

        public bool Start()
        {
            _stitchObject = Activator.CreateInstance(_parameters.StitchType);
            var handlesStart = _stitchObject as IHandlesStart;
            if (handlesStart != null)
                return handlesStart.Start(StitchContext);
            return true;
        }

        public void Stop()
        {
            if (_stitchObject == null)
                return;
            var handlesStop = _stitchObject as IHandlesStop;
            if (handlesStop != null)
                handlesStop.Stop();

            var disposable = _stitchObject as IDisposable;
            if (disposable != null)
                disposable.Dispose();

            _stitchObject = null;
        }

        public void Dispose()
        {
            var disposable = _stitchObject as IDisposable;
            disposable?.Dispose();
        }
    }
}
