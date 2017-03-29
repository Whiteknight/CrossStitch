using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Acquaintance;
using CrossStitch.Backplane.Zyre;
using CrossStitch.Core;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Logging;
using CrossStitch.Core.Modules.Stitches;
using CrossStitch.Core.Utility;

namespace StitchIntegration.ServerA
{
    class Program
    {
        private static IModuleLog _testLog;

        static void Main(string[] args)
        {
            var nodeConfig = NodeConfiguration.GetDefault();
            nodeConfig.NodeId = "ServerA";
            nodeConfig.NodeName = "StitchIntegration.ServerA";
            using (var core = new CrossStitchCore(nodeConfig))
            {
                _testLog = new ModuleLog(core.MessageBus, "ServerA");
                core.AddModule(new BackplaneModule(core));
                core.AddModule(new StitchesModule(core));
                core.AddModule(new LoggingModule(core, Common.Logging.LogManager.GetLogger("CrossStitch")));

                core.Start();

                _testLog.LogInformation("Waiting for ServerB");
                // Wait until we get a node joined event with ServerB. Kick off the test and unsubscribe
                core.MessageBus.Subscribe<ClusterMemberEvent>(b => b
                    .WithChannelName(ClusterMemberEvent.EnteringEvent)
                    .Invoke(m => TestStep1(core.MessageBus, m.NodeId, m.NetworkNodeId))
                    .OnThreadPool()
                    .MaximumEvents(1));

                Console.ReadKey();

                core.Stop();
            }
        }

        private static StitchGroupName _groupName;

        private static void TestStep1(IMessageBus messageBus, string serverBNodeId, string serverBNetworkId)
        {
            Thread.Sleep(5000);
            _testLog.LogInformation("ServerB has joined the cluster.");

            // Zip up the Stitch.js file
            _testLog.LogInformation("Zipping the stitch");
            var stream = new MemoryStream();
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                var entry = zip.CreateEntry("Stitch.js");

                using (var entryStream = entry.Open())
                using (var writer = new StreamWriter(entryStream))
                {
                    writer.Write(File.ReadAllText(".\\Stitch.js"));
                }
            }
            stream.Seek(0, SeekOrigin.Begin);

            // Subscribe to the Job complete event, so we can move to the next step as soon as it is ready
            messageBus.Subscribe<JobCompleteEvent>(b => b
                .WithChannelName(JobCompleteEvent.ChannelSuccess)
                .Invoke(m => TestStep2(messageBus, m))
                .OnThreadPool()
                .MaximumEvents(1));

            // "Upload" the stitch file to ServerA, which will broadcast to ServerB
            _testLog.LogInformation("Uploading the Stitch.zip package file");
            var response = messageBus.Request<PackageFileUploadRequest, PackageFileUploadResponse>(new PackageFileUploadRequest
            {
                Contents = stream,
                FileName = "Stitch.zip",
                GroupName = new StitchGroupName("StitchIntegration.Stitch"),
                LocalOnly = false
            });
            _groupName = response.GroupName;
            _testLog.LogInformation("Uploaded version {0}", _groupName);

            if (!response.Success)
                _testLog.LogError("Could not upload package file");
        }

        private static void TestStep2(IMessageBus messageBus, JobCompleteEvent obj)
        {
            _testLog.LogInformation("Sending create command to ServerB");

            // Subscribe to the Job complete event, so we can move to the next step as soon as it is ready
            messageBus.Subscribe<JobCompleteEvent>(b => b
                .WithChannelName(JobCompleteEvent.ChannelSuccess)
                .Invoke(m => TestStep3(messageBus, m))
                .OnThreadPool()
                .MaximumEvents(1));

            // Command ServerB to create a new stitch instance
            var response = messageBus.Request<CreateInstanceRequest, CreateInstanceResponse>(new CreateInstanceRequest
            {
                Adaptor = new InstanceAdaptorDetails
                {
                    RequiresPackageUnzip = true,
                    Type = AdaptorType.ProcessV1
                },
                GroupName = _groupName,
                LocalOnly = false,
                Name = "Stitch",
                NumberOfInstances = 2
            });
        }

        private static void TestStep3(IMessageBus messageBus, JobCompleteEvent obj)
        {
            // Command ServerB to start the stitch instance
            // Do something to prove that it works as expected
        }
    }
}
