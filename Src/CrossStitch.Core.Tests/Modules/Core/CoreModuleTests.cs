using Acquaintance;
using CrossStitch.Core.Messages.Core;
using CrossStitch.Core.Modules;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace CrossStitch.Core.Tests.Modules.Core
{
    [TestFixture]
    public class CoreModuleTests
    {
        [Test]
        public void ModuleStatusRequest_Test()
        {
            var config = new NodeConfiguration
            {
                NodeId = "TEST",
                NodeName = "TEST"
            };
            var core = new CrossStitchCore(config);
            var target = core.CoreModule;
            target.Start();
            var response = core.MessageBus.Request<ModuleStatusRequest, ModuleStatusResponse>(new ModuleStatusRequest
            {
                ModuleName = "Core"
            });
            response.Should().NotBeNull();
            response.ModuleName.Should().Be("Core");
            response.Found.Should().BeTrue();
            response.StatusValues["Modules"].Should().Be("Core,");
            response.StatusValues["NodeId"].Should().Be("TEST");
        }

        private class DummyModule : IModule
        {
            public void Dispose() { }

            public string Name => "DUMMY";
            public void Start() { }

            public void Stop() { }

            public IReadOnlyDictionary<string, string> GetStatusDetails()
            {
                return new Dictionary<string, string>
                {
                    { "TEST", "TRUE" }
                };
            }
        }

        [Test]
        public void ModuleStatusRequest_CoreWithDummy_Test()
        {
            var config = new NodeConfiguration
            {
                NodeId = "TEST",
                NodeName = "TEST"
            };
            var core = new CrossStitchCore(config);
            core.AddModule(new DummyModule());
            var target = core.CoreModule;
            target.Start();
            var response = core.MessageBus.Request<ModuleStatusRequest, ModuleStatusResponse>(new ModuleStatusRequest
            {
                ModuleName = "Core"
            });
            response.Should().NotBeNull();
            response.ModuleName.Should().Be("Core");
            response.Found.Should().BeTrue();
            response.StatusValues["Modules"].Should().Be("Core,DUMMY");
            response.StatusValues["NodeId"].Should().Be("TEST");
        }

        [Test]
        public void ModuleStatusRequest_Dummy_Test()
        {
            var config = new NodeConfiguration
            {
                NodeId = "TEST",
                NodeName = "TEST"
            };
            var core = new CrossStitchCore(config);
            core.AddModule(new DummyModule());
            var target = core.CoreModule;
            target.Start();
            var response = core.MessageBus.Request<ModuleStatusRequest, ModuleStatusResponse>(new ModuleStatusRequest
            {
                ModuleName = "DUMMY"
            });
            response.Should().NotBeNull();
            response.ModuleName.Should().Be("DUMMY");
            response.Found.Should().BeTrue();
            response.StatusValues["TEST"].Should().Be("TRUE");
        }
    }
}
