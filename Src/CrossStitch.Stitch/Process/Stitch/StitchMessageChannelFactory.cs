using CrossStitch.Stitch.Process.Pipes;
using CrossStitch.Stitch.Process.Stdio;

namespace CrossStitch.Stitch.Process.Stitch
{
    public class StitchMessageChannelFactory
    {
        private readonly string _csNodeId;
        private readonly string _stitchInstanceId;

        public StitchMessageChannelFactory(string csNodeId, string stitchInstanceId)
        {
            _csNodeId = csNodeId;
            _stitchInstanceId = stitchInstanceId;
        }

        public IMessageChannel Create(MessageChannelType channelType)
        {
            if (channelType == MessageChannelType.Pipe)
            {
                var pipeName = PipeMessageChannel.GetPipeName(_csNodeId, _stitchInstanceId);
                return new StitchPipeMessageChannel(pipeName);
            }
            return new StdioMessageChannel();
        }
    }
}