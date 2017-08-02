using System.Text;
using CrossStitch.Core.Models;
using CrossStitch.Stitch;
using CrossStitch.Stitch.Process;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.Process
{
    public class ProcessArguments
    {
        private readonly CoreStitchContext _stitchContext;

        public ProcessArguments(CoreStitchContext stitchContext)
        {
            _stitchContext = stitchContext;
        }

        public string BuildCoreArgumentsString(StitchInstance stitchInstance, string nodeId, int parentPid, MessageChannelType channelType, MessageSerializerType serializerType)
        {
            var sb = new StringBuilder();

            AddArgument(sb, Arguments.CodeId, nodeId);
            AddArgument(sb, Arguments.CorePid, parentPid.ToString());
            AddArgument(sb, Arguments.InstanceId, stitchInstance.Id);
            AddArgument(sb, Arguments.Application, stitchInstance.GroupName.Application);
            AddArgument(sb, Arguments.Component, stitchInstance.GroupName.Component);
            AddArgument(sb, Arguments.Version, stitchInstance.GroupName.Version);
            AddArgument(sb, Arguments.GroupName, stitchInstance.GroupName.ToString());
            AddArgument(sb, Arguments.DataDirectory, _stitchContext.DataDirectory);
            AddArgument(sb, Arguments.ChannelType, channelType.ToString());
            AddArgument(sb, Arguments.Serializer, serializerType.ToString());

            return sb.ToString();
        }

        private void AddArgument(StringBuilder sb, string name, string value)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
                return;
            sb.Append(name);
            sb.Append("=");
            sb.Append(value);
            sb.Append(" ");
        }
    }
}