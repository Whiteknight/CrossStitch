using CrossStitch.Core.Models;
using CrossStitch.Core.Utility.Extensions;
using CrossStitch.Stitch.ProcessV1;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.ProcessV1
{
    public class ProcessV1Parameters
    {
        public ProcessV1Parameters(StitchesConfiguration configuration, StitchFileSystem fileSystem, StitchInstance stitch, Dictionary<string, string> parameters)
        {
            // Use the dir if specified, otherwise default to the running dir from the file system
            var defaultRunningDir = fileSystem.GetInstanceRunningDirectory(stitch.Id);
            DirectoryPath = parameters.GetOrAdd(Parameters.DirectoryPath, defaultRunningDir);

            // Executable name must be specified. Get it and validate
            ExecutableName = parameters.GetOrDefault(Parameters.ExecutableName);
            if (string.IsNullOrEmpty(ExecutableName))
                throw new Exception("Stitch executable name is not specified");

            // Custom args are optional
            ExecutableArguments = parameters.GetOrAdd(Parameters.ExecutableArguments, "") ?? "";

            string executableExt = Path.GetExtension(ExecutableName).ToLower();
            var extConfig = configuration.Extensions.GetOrDefault(executableExt, new StitchesExtensionConfiguration());

            // Use the format from the StitchInstance if specified, otherwise the one configured
            // for the extension, otherwise fall back to the default.
            string executableFormat = "{DirectoryPath}\\{ExecutableName}";
            if (!string.IsNullOrEmpty(extConfig.ExecutableFormat))
                executableFormat = extConfig.ExecutableFormat;
            ExecutableFormat = parameters.GetOrAdd(Parameters.ExecutableFormat, executableFormat);

            // Use the format from the StitchInstance if specified, otherwise the one configured
            // for the extension, otherwise fall back to the default.
            string argsFormat = "{CoreArgs} -- {CustomArgs}";
            if (!string.IsNullOrEmpty(extConfig.ArgumentsFormat))
                argsFormat = extConfig.ArgumentsFormat;
            ArgumentsFormat = parameters.GetOrAdd(Parameters.ArgumentsFormat, argsFormat);
        }

        public string DirectoryPath { get; set; }
        public string ExecutableName { get; set; }
        public string ExecutableArguments { get; set; }

        public string ExecutableFormat { get; set; }
        public string ArgumentsFormat { get; set; }
    }
}