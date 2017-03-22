using CrossStitch.Core.Messages.Backplane;
using FluentAssertions;
using NUnit.Framework;

namespace CrossStitch.Core.Tests.Messages
{
    [TestFixture]
    public class ClusterMessageBuilderTests
    {
        [Test]
        public void Build_Test()
        {
            var target = new ClusterMessageBuilder();

            var result = target.Build();
            result.Should().NotBeNull();
            result.Header.Should().NotBeNull();
        }

        [Test]
        public void Build_WithPayloadObject()
        {
            var target = new ClusterMessageBuilder();
            var obj = new object();
            target.WithObjectPayload(obj);
            var result = target.Build();
            result.PayloadObject.Should().BeSameAs(obj);
        }

        [Test]
        public void Build_ToCluster()
        {
            var target = new ClusterMessageBuilder();
            target.ToCluster();
            var result = target.Build();
            result.Header.ToType.Should().Be(TargetType.Cluster);
        }

        [Test]
        public void Build_ToNode()
        {
            var target = new ClusterMessageBuilder();
            target.ToNode("ABC");
            var result = target.Build();
            result.Header.ToType.Should().Be(TargetType.Node);
            result.Header.ToNetworkId.Should().Be("ABC");
        }

        [Test]
        public void Build_ToZone()
        {
            var target = new ClusterMessageBuilder();
            target.ToZone("ABC");
            var result = target.Build();
            result.Header.ToType.Should().Be(TargetType.Zone);
            result.Header.ZoneName.Should().Be("ABC");
        }

        [Test]
        public void Build_FromNode()
        {
            var target = new ClusterMessageBuilder();
            target.FromNode();
            var result = target.Build();
            result.Header.FromType.Should().Be(TargetType.Node);
        }
    }
}
