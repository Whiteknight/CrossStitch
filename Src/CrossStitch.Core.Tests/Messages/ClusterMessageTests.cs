using System.Collections.Generic;
using CrossStitch.Core.Messages.Backplane;
using CrossStitch.Core.Models;
using FluentAssertions;
using NUnit.Framework;

namespace CrossStitch.Core.Tests.Messages
{
    [TestFixture]
    public class ClusterMessageTests
    {
        [Test]
        public void IsSendable_Default()
        {
            var target = new ClusterMessage();
            target.IsSendable().Should().BeFalse();
        }

        [Test]
        public void IsSendable_NoHeader()
        {
            var target = new ClusterMessage();
            target.Header = null;
            target.IsSendable().Should().BeFalse();
        }

        [Test]
        public void IsSendable_NoReturnAddress()
        {
            var target = new ClusterMessage();
            target.Header.PayloadType = MessagePayloadType.Object;
            target.PayloadObject = new object();
            target.IsSendable().Should().BeFalse();
        }

        [Test]
        public void IsSendable_ToNodeNoNodeId()
        {
            var target = new ClusterMessage();
            target.Header.PayloadType = MessagePayloadType.Object;
            target.Header.FromNetworkId = "ABC";
            target.Header.FromNodeId = "123";
            target.PayloadObject = new object();
            target.Header.ToType = TargetType.Node;

            target.IsSendable().Should().BeFalse();
        }

        [Test]
        public void IsSendable_ToZoneNoZone()
        {
            var target = new ClusterMessage();
            target.Header.PayloadType = MessagePayloadType.Object;
            target.Header.FromNetworkId = "ABC";
            target.Header.FromNodeId = "123";
            target.PayloadObject = new object();
            target.Header.ToType = TargetType.Zone;

            target.IsSendable().Should().BeFalse();
        }

        [Test]
        public void IsSendable_ObjectPayloadNull()
        {
            var target = new ClusterMessage();
            target.Header.PayloadType = MessagePayloadType.Object;
            target.PayloadObject = null;

            target.IsSendable().Should().BeFalse();
        }

        [Test]
        public void IsSendable_CommandStringNull()
        {
            var target = new ClusterMessage();
            target.Header.PayloadType = MessagePayloadType.CommandString;
            target.CommandStrings = null;

            target.IsSendable().Should().BeFalse();
        }

        [Test]
        public void IsSendable_CommandStringEmpty()
        {
            var target = new ClusterMessage();
            target.Header.PayloadType = MessagePayloadType.CommandString;
            target.CommandStrings = new List<string>();

            target.IsSendable().Should().BeFalse();
        }

        [Test]
        public void IsSendable_RawFramesNull()
        {
            var target = new ClusterMessage();
            target.Header.PayloadType = MessagePayloadType.Raw;
            target.RawFrames = null;

            target.IsSendable().Should().BeFalse();
        }

        [Test]
        public void IsSendable_RawFramesEmpty()
        {
            var target = new ClusterMessage();
            target.Header.PayloadType = MessagePayloadType.Raw;
            target.RawFrames = new List<byte[]>();

            target.IsSendable().Should().BeFalse();
        }

        [Test]
        public void FillInNetworkNodeId_Test()
        {
            var target = new ClusterMessage();
            target.Header.PayloadType = MessagePayloadType.Object;
            target.PayloadObject = new NodeStatus();
            target.FillInNetworkNodeId("ABC");
            var ns = (NodeStatus)target.PayloadObject;
            ns.NetworkNodeId.Should().Be("ABC");
        }

        [Test]
        public void GetReceiveEventName_Test()
        {
            var target = new ClusterMessage();
            target.Header.EventName = "TEST";
            target.GetReceiveEventName().Should().Be("Received.TEST");
        }

        [Test]
        public void GetReceiveEventName_NoEventName()
        {
            var target = new ClusterMessage();
            target.GetReceiveEventName().Should().Be("Received");
        }
    }
}
