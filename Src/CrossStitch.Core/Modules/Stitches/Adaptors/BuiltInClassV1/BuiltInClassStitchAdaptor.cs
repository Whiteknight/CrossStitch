using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Stitch.BuiltInClassV1;
using System;
using CrossStitch.Stitch;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.BuiltInClassV1
{
    public class BuiltInClassV1StitchAdaptor : IStitchAdaptor
    {
        private readonly StitchInstance _stitchInstance;
        private readonly BuiltInClassV1Parameters _parameters;
        private object _stitchObject;

        public BuiltInClassV1StitchAdaptor(PackageFile packageFile, StitchInstance stitchInstance, CoreStitchContext stitchContext)
        {
            StitchContext = stitchContext;
            _stitchInstance = stitchInstance;
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
            return handlesStart == null || handlesStart.Start(StitchContext);
        }

        public void Stop()
        {
            if (_stitchObject == null)
                return;

            var handlesStop = _stitchObject as IHandlesStop;
            handlesStop?.Stop();

            var disposable = _stitchObject as IDisposable;
            disposable?.Dispose();

            _stitchObject = null;
        }

        public void Dispose()
        {
            var disposable = _stitchObject as IDisposable;
            disposable?.Dispose();
        }
    }
}
