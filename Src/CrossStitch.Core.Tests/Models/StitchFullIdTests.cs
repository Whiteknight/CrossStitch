using CrossStitch.Core.Models;
using FluentAssertions;
using NUnit.Framework;

namespace CrossStitch.Core.Tests.Models
{
    [TestFixture]
    public class StitchFullIdTests
    {
        [Test]
        public void StitchFullId_FullId()
        {
            var target = new StitchFullId("A:B");
            target.IsLocalOnly.Should().Be(false);
            target.NodeId.Should().Be("A");
            target.StitchInstanceId.Should().Be("B");
        }

        [Test]
        public void StitchFullId_FullIdLocal()
        {
            var target = new StitchFullId("B");
            target.IsLocalOnly.Should().Be(true);
            target.NodeId.Should().Be(string.Empty);
            target.StitchInstanceId.Should().Be("B");
        }

        [Test]
        public void StitchFullId_SeparateIds()
        {
            var target = new StitchFullId("A", "B");
            target.IsLocalOnly.Should().Be(false);
            target.NodeId.Should().Be("A");
            target.StitchInstanceId.Should().Be("B");
        }

        [Test]
        public void StitchFullId_FullIdInStitchId()
        {
            var target = new StitchFullId(null, "A:B");
            target.IsLocalOnly.Should().Be(false);
            target.NodeId.Should().Be("A");
            target.StitchInstanceId.Should().Be("B");
        }

        [Test]
        public void StitchFullId_NodeIdNull()
        {
            var target = new StitchFullId(null, "B");
            target.IsLocalOnly.Should().Be(true);
            target.NodeId.Should().Be(string.Empty);
            target.StitchInstanceId.Should().Be("B");
        }

        [Test]
        public void StitchFullId_NodeIdEmpty()
        {
            var target = new StitchFullId(string.Empty, "B");
            target.IsLocalOnly.Should().Be(true);
            target.NodeId.Should().Be(string.Empty);
            target.StitchInstanceId.Should().Be("B");
        }
    }
}
