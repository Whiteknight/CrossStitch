using System.Text;
using CrossStitch.Core.Models;
using CrossStitch.Stitch.ProcessV1;
using CrossStitch.Stitch.ProcessV1.Core;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.ProcessV1
{
    public class ProcessV1ArgsBuilder
    {
        private readonly CoreStitchContext _stitchContext;

        public ProcessV1ArgsBuilder(CoreStitchContext stitchContext)
        {
            _stitchContext = stitchContext;
        }

        public string BuildCoreArgumentsString(StitchInstance stitchInstance, int parentPid)
        {
            var sb = new StringBuilder();

            AddArgument(sb, Arguments.CorePid, parentPid.ToString());
            AddArgument(sb, Arguments.InstanceId, stitchInstance.Id);
            AddArgument(sb, Arguments.Application, stitchInstance.GroupName.Application);
            AddArgument(sb, Arguments.Component, stitchInstance.GroupName.Component);
            AddArgument(sb, Arguments.Version, stitchInstance.GroupName.Version);
            AddArgument(sb, Arguments.GroupName, stitchInstance.GroupName.ToString());
            AddArgument(sb, Arguments.DataDirectory, _stitchContext.DataDirectory);

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