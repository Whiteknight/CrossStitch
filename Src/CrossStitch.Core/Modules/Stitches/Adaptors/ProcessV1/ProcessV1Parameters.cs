using System.Collections.Generic;
using CrossStitch.Core.Utility.Extensions;
using CrossStitch.Stitch.ProcessV1;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.ProcessV1
{
    public class ProcessV1Parameters
    {
        public ProcessV1Parameters(Dictionary<string, string> parameters)
        {
            DirectoryPath = parameters.GetOrDefault(Parameters.DirectoryPath);
            ExecutableName = parameters.GetOrDefault(Parameters.ExecutableName);
            ExecutableArguments = parameters.GetOrDefault(Parameters.ExecutableArguments);
        }

        public string DirectoryPath { get; set; }
        public string ExecutableName { get; set; }
        public string ExecutableArguments { get; set; }
    }
}