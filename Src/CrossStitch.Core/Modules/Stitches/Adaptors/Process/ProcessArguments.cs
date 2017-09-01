using System.Text;
using CrossStitch.Core.Models;
using CrossStitch.Stitch.Process;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.Process
{
    public class ProcessArguments
    {
        public string BuildCoreArgumentsString(StitchInstance stitchInstance, CrossStitchCore core, ProcessParameters parameters)
        {
            var sb = new StringBuilder();

            AddArgument(sb, Arguments.CoreId, core.NodeId);
            AddArgument(sb, Arguments.CorePid, core.CorePid.ToString());
            AddArgument(sb, Arguments.InstanceId, stitchInstance.Id);
            AddArgument(sb, Arguments.Application, stitchInstance.GroupName.Application);
            AddArgument(sb, Arguments.Component, stitchInstance.GroupName.Component);
            AddArgument(sb, Arguments.Version, stitchInstance.GroupName.Version);
            AddArgument(sb, Arguments.GroupName, stitchInstance.GroupName.ToString());
            AddArgument(sb, Arguments.DataDirectory, parameters.DataDirectory);
            AddArgument(sb, Arguments.ChannelType, parameters.ChannelType.ToString());
            AddArgument(sb, Arguments.Serializer, parameters.SerializerType.ToString());

            return sb.ToString();
        }

        private static void AddArgument(StringBuilder sb, string name, string value)
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