﻿using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master;
using FluentAssertions;
using NUnit.Framework;

namespace CrossStitch.Core.Tests.Modules.Master
{
    [TestFixture]
    public class NodeStatusBuilderTests
    {
        [Test]
        public void Build_Test()
        {
            var modules = new[] { "A", "B", "C" };
            var stitches = new StitchInstance[]
            {
                new StitchInstance
                {
                    Id = "1",
                    GroupName = new StitchGroupName("A.B.C"),
                    State = InstanceStateType.Running
                },
                new StitchInstance
                {
                    Id = "2",
                    GroupName = new StitchGroupName("A.B.C"),
                    State = InstanceStateType.Started
                },
                new StitchInstance
                {
                    Id = "3",
                    GroupName = new StitchGroupName("A.B.C"),
                    State = InstanceStateType.Stopped
                }
            };
            var zones = new[] { "_all", "some", "none" };
            var target = new NodeStatusBuilder("NodeId", "NodeName", "NetworkNodeId", zones, modules, stitches);
            var result = target.Build();

            result.Id.Should().Be("NodeId");
            result.Name.Should().Be("NodeName");
            result.NetworkNodeId.Should().Be("NetworkNodeId");
            result.RunningModules.Should().Contain(modules);
            // TODO: We should keep track of zones in the Master module and fill this in
            result.Zones.Count.Should().Be(3);
            result.StitchInstances.Count.Should().Be(3);

        }
    }
}
