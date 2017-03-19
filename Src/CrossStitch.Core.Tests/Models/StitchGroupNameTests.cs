using CrossStitch.Core.Models;
using FluentAssertions;
using NUnit.Framework;

namespace CrossStitch.Core.Tests.Models
{
    [TestFixture]
    public class StitchGroupNameTests
    {
        [Test]
        public void ConstructFromGroupName_Version_Test()
        {
            var target = new StitchGroupName("A.B.C");
            target.Application.Should().Be("A");
            target.Component.Should().Be("B");
            target.Version.Should().Be("C");
        }

        [Test]
        public void ConstructFromGroupName_Component_Test()
        {
            var target = new StitchGroupName("A.B");
            target.Application.Should().Be("A");
            target.Component.Should().Be("B");
            target.Version.Should().BeNull();
        }

        [Test]
        public void ConstructFromGroupName_Application_Test()
        {
            var target = new StitchGroupName("A");
            target.Application.Should().Be("A");
            target.Component.Should().BeNull();
            target.Version.Should().BeNull();
        }

        [Test]
        public void ConstructFromParts_Application_Test()
        {
            var target = new StitchGroupName("A", null, null);
            target.Application.Should().Be("A");
            target.Component.Should().BeNull();
            target.Version.Should().BeNull();
        }

        [Test]
        public void ConstructFromParts_Component_Test()
        {
            var target = new StitchGroupName("A", "B", null);
            target.Application.Should().Be("A");
            target.Component.Should().Be("B");
            target.Version.Should().BeNull();
        }

        [Test]
        public void ConstructFromParts_Version_Test()
        {
            var target = new StitchGroupName("A", "B", "C");
            target.Application.Should().Be("A");
            target.Component.Should().Be("B");
            target.Version.Should().Be("C");
        }

        [Test]
        public void ToString_Test()
        {
            var target = new StitchGroupName("A.B.C");
            target.ToString().Should().Be("A.B.C");

            target = new StitchGroupName("A", "B", "C");
            target.ToString().Should().Be("A.B.C");
        }

        [Test]
        public void IsValid_Test()
        {
            var target = new StitchGroupName()
            {
                VersionString = null
            };
            target.IsValid().Should().BeFalse();
        }

        [Test]
        public void IsApplicationGroup_Test()
        {
            new StitchGroupName("A").IsApplicationGroup().Should().BeTrue();
            new StitchGroupName("A.B").IsApplicationGroup().Should().BeFalse();
        }

        [Test]
        public void IsComponentGroup_Test()
        {
            new StitchGroupName("A.B").IsComponentGroup().Should().BeTrue();
            new StitchGroupName("A").IsComponentGroup().Should().BeFalse();
            new StitchGroupName("A.B.C").IsComponentGroup().Should().BeFalse();
        }

        [Test]
        public void IsVersionGroup_Test()
        {
            new StitchGroupName("A.B.C").IsVersionGroup().Should().BeTrue();
            new StitchGroupName("A").IsVersionGroup().Should().BeFalse();
            new StitchGroupName("A.B").IsVersionGroup().Should().BeFalse();
        }

        [Test]
        public void Contains_Test()
        {
            new StitchGroupName("A").Contains(new StitchGroupName("A")).Should().BeTrue();
            new StitchGroupName("A").Contains(new StitchGroupName("A.B")).Should().BeTrue();
            new StitchGroupName("A").Contains(new StitchGroupName("A.B.C")).Should().BeTrue();
            new StitchGroupName("A").Contains(new StitchGroupName("B")).Should().BeFalse();

            new StitchGroupName("A.B").Contains(new StitchGroupName("A.B")).Should().BeTrue();
            new StitchGroupName("A.B").Contains(new StitchGroupName("A.B.C")).Should().BeTrue();
            new StitchGroupName("A.B").Contains(new StitchGroupName("A")).Should().BeFalse();
            new StitchGroupName("A.B").Contains(new StitchGroupName("A.C")).Should().BeFalse();

            new StitchGroupName("A.B.C").Contains(new StitchGroupName("A.B.C")).Should().BeTrue();
            new StitchGroupName("A.B.C").Contains(new StitchGroupName("A")).Should().BeFalse();
            new StitchGroupName("A.B.C").Contains(new StitchGroupName("A.B")).Should().BeFalse();
            new StitchGroupName("A.B.C").Contains(new StitchGroupName("A.B.D")).Should().BeFalse();
        }

        [Test]
        public void Contains_string_Test()
        {
            new StitchGroupName("A").Contains("A").Should().BeTrue();
            new StitchGroupName("A").Contains("A.B").Should().BeTrue();
            new StitchGroupName("A").Contains("A.B.C").Should().BeTrue();
            new StitchGroupName("A").Contains("B").Should().BeFalse();

            new StitchGroupName("A.B").Contains("A.B").Should().BeTrue();
            new StitchGroupName("A.B").Contains("A.B.C").Should().BeTrue();
            new StitchGroupName("A.B").Contains("A").Should().BeFalse();
            new StitchGroupName("A.B").Contains("A.C").Should().BeFalse();

            new StitchGroupName("A.B.C").Contains("A.B.C").Should().BeTrue();
            new StitchGroupName("A.B.C").Contains("A").Should().BeFalse();
            new StitchGroupName("A.B.C").Contains("A.B").Should().BeFalse();
            new StitchGroupName("A.B.C").Contains("A.B.D").Should().BeFalse();
        }
    }
}
