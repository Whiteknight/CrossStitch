using System;
using System.Diagnostics;
using CrossStitch.Core.Models;
using CrossStitch.Core.Utility;
using CrossStitch.Stitch;
using CrossStitch.Stitch.Process;
using CrossStitch.Stitch.Utility;

namespace CrossStitch.Core.Modules.Stitches.Adaptors.Process
{
    public class ProcessFactory : IFactory<System.Diagnostics.Process, ProcessParameters>
    {
        private readonly StitchInstance _stitchInstance;
        private readonly CrossStitchCore _core;
        private readonly CoreStitchContext _stitchContext;
        private readonly IModuleLog _log;

        public ProcessFactory(StitchInstance stitchInstance, CrossStitchCore core, CoreStitchContext stitchContext, IModuleLog log)
        {
            _stitchInstance = stitchInstance;
            _core = core;
            _stitchContext = stitchContext;
            _log = log;
        }

        public System.Diagnostics.Process Create(ProcessParameters parameters)
        {
            try
            {
                //var executableName = Path.Combine(_parameters.DirectoryPath, _parameters.ExecutableName);
                var process = CreateNewProcessInternal(parameters);

                if (!process.Start())
                {
                    _log.LogError("Process could not be started.");
                    process.Dispose();
                    return null;
                }

                return process;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Process could not be created");
                return null;
            }
        }

        private System.Diagnostics.Process CreateNewProcessInternal(ProcessParameters parameters)
        {
            bool useStdio = parameters.ChannelType == MessageChannelType.Stdio;

            var executableFile = parameters.ExecutableFormat
                .Replace("{ExecutableName}", parameters.ExecutableName)
                .Replace("{DirectoryPath}", parameters.DirectoryPath);

            var coreArgs = new ProcessArguments(_stitchContext).BuildCoreArgumentsString(_stitchInstance, _core, parameters);
            var arguments = parameters.ArgumentsFormat
                .Replace("{ExecutableName}", parameters.ExecutableName)
                .Replace("{DirectoryPath}", parameters.DirectoryPath)
                .Replace("{CoreArgs}", coreArgs)
                .Replace("{CustomArgs}", parameters.ExecutableArguments);

            var process = new System.Diagnostics.Process
            {
                EnableRaisingEvents = true,
                StartInfo =
                {
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    FileName = executableFile,
                    WorkingDirectory = parameters.DirectoryPath,
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