using System.Collections.Generic;
using System.Linq;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Messages.Stitches;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master;
using FluentAssertions;
using NUnit.Framework;

namespace CrossStitch.Core.Tests.Modules.Master
{
    [TestFixture]
    public class MasterStitchCacheTests
    {
        [Test]
        public void GetStitchSummaries_Empty()
        {
            var target = new MasterStitchCache("1", null, null);
            var result = target.GetStitchSummaries();
            result.Count.Should().Be(0);

            // Do it a second time, because we cache result lists and want to make sure nothing changes
            result = target.GetStitchSummaries();
            result.Count.Should().Be(0);
        }

        [Test]
        public void AddLocalStitch_Test()
        {
            var target = new MasterStitchCache("1", null, null);
            target.AddLocalStitch("ABC", new StitchGroupName("A.B.C"));
            var result = target.GetStitchSummaries();
            result.Count.Should().Be(1);
            result[0].Id.Should().Be("ABC");
        }

        [Test]
        public void AddLocalStitch_Replace()
        {
            var target = new MasterStitchCache("1", null, null);
            target.AddLocalStitch("ABC", new StitchGroupName("A.B.C"));
            var result = target.GetStitchSummaries();
            result.Count.Should().Be(1);
            result[0].Id.Should().Be("ABC");

            target.AddLocalStitch("ABC", new StitchGroupName("A.B.D"));
            result = target.GetStitchSummaries();
            result.Count.Should().Be(1);
            result[0].Id.Should().Be("ABC");
            result[0].GroupName.VersionString.Should().Be("A.B.D");
        }

        [Test]
        public void RemoveLocalStitch_Test()
        {
            var target = new MasterStitchCache("1", null, null);
            target.AddLocalStitch("ABC", new StitchGroupName("A.B.C"));
            target.RemoveLocalStitch("ABC");
            var result = target.GetStitchSummaries();
            result.Count.Should().Be(0);
        }

        [Test]
        public void AddNodeStatus_Test()
        {
            var target = new MasterStitchCache("1", null, null);
            var received = new ObjectReceivedEvent<NodeStatus>
            {
                FromNetworkId = "RemoteNetworkId",
                FromNodeId = "RemoteNodeId",
                Object = new NodeStatus
                {
                    Id = "RemoteNodeId",
                    Name = "RemoteNodeName",
                    NetworkNodeId = "RemoteNetworkId",
                    StitchInstances = new List<InstanceInformation>
                    {
                        new InstanceInformation
                        {
                            GroupName = "A.B.C",
                            Id = "1",
                            State = InstanceStateType.Running
                        },
                        new InstanceInformation
                        {
                            GroupName = "A.B.C",
                            Id = "2",
                            State = InstanceStateType.Running
                        },
                        new InstanceInformation
                        {
                            GroupName = "A.B.C",
                            Id = "3",
                            State = InstanceStateType.Stopped
                        }
                    }
                }
            };
            target.AddNodeStatus(received, received.Object);

            var result = target.GetStitchSummaries().OrderBy(ss => ss.Id).ToList();
            result.Count.Should().Be(2);
            result[0].Id.Should().Be("1");
            result[1].Id.Should().Be("2");

            result.Should().OnlyContain(ss => ss.GroupName.VersionString == "A.B.C");
            result.Should().OnlyContain(ss => ss.NodeId == "RemoteNodeId");
            result.Should().OnlyContain(ss => ss.NetworkNodeId == "RemoteNetworkId");
        }

        [Test]
        public void AddNodeStatus_ReceivedAgain()
        {
            var target = new MasterStitchCache("1", null, null);
            var received = new ObjectReceivedEvent<NodeStatus>
            {
                FromNetworkId = "RemoteNetworkId",
                FromNodeId = "RemoteNodeId",
                Object = new NodeStatus
                {
                    Id = "RemoteNodeId",
                    Name = "RemoteNodeName",
                    NetworkNodeId = "RemoteNetworkId",
                    StitchInstances = new List<InstanceInformation>
                    {
                    }
                }
            };

            target.AddNodeStatus(received, received.Object);
            received = new ObjectReceivedEvent<NodeStatus>
            {
                FromNetworkId = "RemoteNetworkId",
                FromNodeId = "RemoteNodeId",
                Object = new NodeStatus
                {
                    Id = "RemoteNodeId",
                    Name = "RemoteNodeName",
                    NetworkNodeId = "RemoteNetworkId",
                    StitchInstances = new List<InstanceInformation>
                    {
                        new InstanceInformation
                        {
                            GroupName = "A.B.C",
                            Id = "1",
                            State = InstanceStateType.Running
                        },
                        new InstanceInformation
                        {
                            GroupName = "A.B.C",
                            Id = "2",
                            State = InstanceStateType.Running
                        },
                        new InstanceInformation
                        {
                            GroupName = "A.B.C",
                            Id = "3",
                            State = InstanceStateType.Stopped
                        }
                    }
                }
            };
            target.AddNodeStatus(received, received.Object);

            var result = target.GetStitchSummaries().OrderBy(ss => ss.Id).ToList();
            result.Count.Should().Be(2);
            result[0].Id.Should().Be("1");
            result[1].Id.Should().Be("2");

            result.Should().OnlyContain(ss => ss.GroupName.VersionString == "A.B.C");
            result.Should().OnlyContain(ss => ss.NodeId == "RemoteNodeId");
            result.Should().OnlyContain(ss => ss.NetworkNodeId == "RemoteNetworkId");
        }
    }
}
