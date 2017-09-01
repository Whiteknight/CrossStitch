using System;
using System.IO;
using CrossStitch.Core.Models;
using CrossStitch.Stitch.Process;
using CrossStitch.Stitch.Utility.Extensions;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.Process
{
    public class ProcessParameters
    {
        public ProcessParameters(StitchesConfiguration configuration, StitchFileSystem fileSystem, StitchInstance stitch, PackageFile packageFile)
        {
            var parameters = packageFile.Adaptor.Parameters;
            // Use the dir if specified, otherwise default to the running dir from the file system
            var defaultRunningDir = fileSystem.GetInstanceRunningDirectory(stitch.Id);
            RunningDirectory = parameters.GetOrAdd(Parameters.RunningDirectory, defaultRunningDir);

            var defaultDataDir = fileSystem.GetInstanceDataDirectoryPath(stitch.Id);
            DataDirectory = parameters.GetOrAdd(Parameters.DataDirectory, defaultDataDir);

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

            ChannelType = packageFile.Adaptor.Channel;
            SerializerType = packageFile.Adaptor.Serializer;
        }

        public string RunningDirectory { get; set; }
        public string DataDirectory { get; set; }
        public string ExecutableName { get; set; }
        public string ExecutableArguments { get; set; }

        public string ExecutableFormat { get; set; }
        public string ArgumentsFormat { get; set; }
        public MessageChannelType ChannelType { get; set; }
        public MessageSerializerType SerializerType { get; set; }
    }
}