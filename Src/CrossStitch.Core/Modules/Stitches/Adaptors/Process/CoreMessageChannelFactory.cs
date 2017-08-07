using CrossStitch.Stitch.Process;
using CrossStitch.Stitch.Process.Pipes;
using CrossStitch.Stitch.Process.Stdio;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.Process
{
    public class CoreMessageChannelFactory
    {
        private readonly string _csNodeId;
        private readonly string _stitchInstanceId;

        public CoreMessageChannelFactory(string csNodeId, string stitchInstanceId)
        {
            _csNodeId = csNodeId;
            _stitchInstanceId = stitchInstanceId;
        }

        public IMessageChannel Create(MessageChannelType channelType, System.Diagnostics.Process process)
        {
            if (channelType == MessageChannelType.Pipe)
            {
                var pipeName = PipeMessageChannel.GetPipeName(_csNodeId, _stitchInstanceId);
                return new CorePipeMessageChannel(pipeName);
            }

            return new StdioMessageChannel(process.StandardOutput, process.StandardInput);
        }
    }
}