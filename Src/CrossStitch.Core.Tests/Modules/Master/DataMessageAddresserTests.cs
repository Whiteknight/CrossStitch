using CrossStitch.Core.Messages;
using CrossStitch.Core.Models;
using CrossStitch.Core.Modules.Master;
using CrossStitch.Core.Modules.Master.Models;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Tests.Modules.Master
{
    [TestFixture]
    public class DataMessageAddresserTests
    {
        private static DataMessageAddresser CreateTarget()
        {
            return new DataMessageAddresser(new List<StitchSummary>
            {
                new StitchSummary
                {
                    Id = "1",
                    GroupName = new StitchGroupName("A.B.1"),
                    NodeId = "1",
                    NetworkNodeId = "1N",
                    Locale = StitchLocaleType.Local
                },
                new StitchSummary
                {
                    Id = "2",
                    GroupName = new StitchGroupName("A.B.2"),
                    NodeId = "1",
                    NetworkNodeId = "1N",
                    Locale = StitchLocaleType.Local
                },
                new StitchSummary
                {
                    Id = "3",
                    GroupName = new StitchGroupName("A.B.1"),
                    NodeId = "2",
                    NetworkNodeId = "2N",
                    Locale = StitchLocaleType.Remote
                },
                new StitchSummary
                {
                    Id = "4",
                    GroupName = new StitchGroupName("A.B.2"),
                    NodeId = "2",
                    NetworkNodeId = "2N",
                    Locale = StitchLocaleType.Remote
                }
            });
        }

        [Test]
        public void AddressMessage_Instance()
        {
            var target = CreateTarget();
            var message = new StitchDataMessage
            {
                ToStitchInstanceId = "1",
                Data = "ABC",
                FromNodeId = "FromNode1",
                FromStitchInstanceId = "FromStitch1"
            };
            var result = target.AddressMessage(message).ToList();
            result.Count.Should().Be(1);
            result[0].ToNodeId.Should().Be("1");
            result[0].ToNetworkId.Should().Be("1N");
            result[0].Data.Should().Be("ABC");
        }

        [Test]
        public void AddressMessage_Instance_NotFound()
        {
            var target = CreateTarget();
            var message = new StitchDataMessage
            {
                ToStitchInstanceId = "XXX",
                Data = "ABC",
                FromNodeId = "FromNode1",
                FromStitchInstanceId = "FromStitch1"
            };
            var result = target.AddressMessage(message).ToList();
            result.Count.Should().Be(0);
        }

        [Test]
        public void AddressMessage_Instance_FullToId()
        {
            var target = CreateTarget();
            var message = new StitchDataMessage
            {
                ToStitchInstanceId = "1:1",
                Data = "ABC",
                FromNodeId = "FromNode1",
                FromStitchInstanceId = "FromStitch1"
            };
            var result = target.AddressMessage(message).ToList();
            result.Count.Should().Be(1);
            result[0].ToNodeId.Should().Be("1");
            result[0].ToNetworkId.Should().Be("1N");
            result[0].Data.Should().Be("ABC");
        }

        [Test]
        public void AddressMessage_Instance_FullToIdBadNode()
        {
            var target = CreateTarget();
            var message = new StitchDataMessage
            {
                ToStitchInstanceId = "2:1",
                Data = "ABC",
                FromNodeId = "FromNode1",
                FromStitchInstanceId = "FromStitch1"
            };
            var result = target.AddressMessage(message).ToList();
            result.Count.Should().Be(0);
        }

        [Test]
        public void AddressMessage_Group()
        {
            var target = CreateTarget();
            var message = new StitchDataMessage
            {
                ToStitchGroup = "A.B.1",
                Data = "ABC",
                FromNodeId = "FromNode1",
                FromStitchInstanceId = "FromStitch1"
            };
            var result = target.AddressMessage(message).OrderBy(si => si.Id).ToList();
            result.Count.Should().Be(2);
            result[0].ToStitchInstanceId.Should().Be("1");
            result[0].ToNodeId.Should().Be("1");
            result[0].ToNetworkId.Should().Be("1N");
            result[1].ToStitchInstanceId.Should().Be("3");
            result[1].ToNodeId.Should().Be("2");
            result[1].ToNetworkId.Should().Be("2N");
        }

        [Test]
        public void AddressMessage_Group_NotFound()
        {
            var target = CreateTarget();
            var message = new StitchDataMessage
            {
                ToStitchGroup = "A.B.X",
                Data = "ABC",
                FromNodeId = "FromNode1",
                FromStitchInstanceId = "FromStitch1"
            };
            var result = target.AddressMessage(message).OrderBy(si => si.Id).ToList();
            result.Count.Should().Be(0);
        }

        [Test]
        public void AddressMessage_NoAddressee()
        {
            var target = CreateTarget();
            var message = new StitchDataMessage
            {
                Data = "ABC",
                FromNodeId = "FromNode1",
                FromStitchInstanceId = "FromStitch1"
            };
            var result = target.AddressMessage(message).OrderBy(si => si.Id).ToList();
            result.Count.Should().Be(0);
        }
    }
}
