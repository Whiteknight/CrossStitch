using System;
using System.Diagnostics;
using System.Text;
using CrossStitch.Core.Models;
using CrossStitch.Core.Utility;
using CrossStitch.Stitch.Process;
using CrossStitch.Stitch.Utility;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.Process
{
    public class ProcessFactory : IFactory<System.Diagnostics.Process, ProcessParameters>
    {
        private readonly StitchInstance _stitchInstance;
        private readonly CrossStitchCore _core;
        private readonly IModuleLog _log;

        public ProcessFactory(StitchInstance stitchInstance, CrossStitchCore core, IModuleLog log)
        {
            _stitchInstance = stitchInstance;
            _core = core;
            _log = log;
        }

        public System.Diagnostics.Process Create(ProcessParameters parameters)
        {
            var process = CreateNewProcessInternal(parameters);
            try
            {
                if (!process.Start())
                {
                    _log.LogError("Process could not be started but there was no error. The process may already exist.");
                    process.Dispose();
                    return null;
                }

                return process;
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Process could not be started because of an error");
                sb.AppendLine($"    FileName={process.StartInfo.FileName}");
                sb.AppendLine($"    CurrentDirectory={System.IO.Directory.GetCurrentDirectory()}");
                _log.LogError(e, sb.ToString());
                return null;
            }
        }

        private System.Diagnostics.Process CreateNewProcessInternal(ProcessParameters parameters)
        {
            bool useStdio = parameters.ChannelType == MessageChannelType.Stdio;

            var sb = new StringBuilder();
            sb.AppendLine("Process Create Parameters:");
            sb.AppendLine($"    ExecutableFormat: {parameters.ExecutableFormat}");
            sb.AppendLine($"    ExecutableName: {parameters.ExecutableName}");
            sb.AppendLine($"    RunningDirectory: {parameters.RunningDirectory}");

            var executableFile = parameters.ExecutableFormat
                .Replace("{ExecutableName}", parameters.ExecutableName)
                .Replace("{DirectoryPath}", parameters.RunningDirectory);
            sb.AppendLine($"    ExecutableFile: {executableFile}");

            var coreArgs = new ProcessArguments().BuildCoreArgumentsString(_stitchInstance, _core, parameters);

            sb.AppendLine($"    ArgumentsFormat: {parameters.ArgumentsFormat}");
            sb.AppendLine($"    CoreArgs: {coreArgs}");
            sb.AppendLine($"    CustomArgs: {parameters.ExecutableArguments}");

            var arguments = parameters.ArgumentsFormat
                .Replace("{ExecutableName}", parameters.ExecutableName)
                .Replace("{DirectoryPath}", parameters.RunningDirectory)
                .Replace("{CoreArgs}", coreArgs)
                .Replace("{CustomArgs}", parameters.ExecutableArguments);
            sb.AppendLine($"    Arguments: {arguments}");
            _log.LogDebugRaw(sb.ToString());

            var process = new System.Diagnostics.Process
            {
                EnableRaisingEvents = true,
                StartInfo =
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    FileName = executableFile,
                    WorkingDirectory = parameters.RunningDirectory,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardError = useStdio,
                    RedirectStandardInput = useStdio,
                    RedirectStandardOutput = useStdio,
                    Arguments = arguments
                }
            };

            return process;
        }
    }
}