using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossStitch.Core.Models;
using FluentAssertions;
using NUnit.Framework;

namespace CrossStitch.Core.Tests.Models
{
    [TestFixture]
    public class ApplicationTests
    {
        [Test]
        public void AddComponent_Test()
        {
            var target = new Application();
            target.AddComponent("TEST");
            target.Components.Should().HaveCount(1);
            target.Components[0].Name.Should().Be("TEST");
        }


        [Test]
        public void AddComponent_ReAdd_Test()
        {
            var target = new Application();
            target.AddComponent("TEST");
            target.AddComponent("TEST");
            target.Components.Should().HaveCount(1);
            target.Components[0].Name.Should().Be("TEST");
        }

        [Test]
        public void RemoveComponent_Test()
        {
            var target = new Application();
            target.AddComponent("TEST");
            target.RemoveComponent("TEST");
            target.Components.Should().HaveCount(0);
        }

        [Test]
        public void AddVersion_NoComponent_Test()
        {
            var target = new Application();
            target.AddVersion("TESTC", "TESTV");
            target.Components.Should().HaveCount(0);
        }

        [Test]
        public void AddVersion_Test()
        {
            var target = new Application();
            target.AddComponent("TESTC");
            target.AddVersion("TESTC", "TESTV");
            target.Components[0].Versions.Any(v => v.Version == "TESTV").Should().BeTrue();
        }

        [Test]
        public void HasVersion_Test()
        {
            var target = new Application();
            target.AddComponent("TESTC");
            target.AddVersion("TESTC", "TESTV");
            target.HasVersion("TESTC", "TESTV").Should().BeTrue();
        }
    }
}

