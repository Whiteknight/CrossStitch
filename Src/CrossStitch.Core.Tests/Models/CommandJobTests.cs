using CrossStitch.Core.Messages.Master;
using CrossStitch.Core.Models;
using FluentAssertions;
using NUnit.Framework;

namespace CrossStitch.Core.Tests.Models
{
    [TestFixture]
    public class CommandJobTests
    {
        [Test]
        public void Status_Test()
        {
            var target = new CommandJob();
            target.Status.Should().Be(JobStatusType.Success);

            target.Tasks.Add(new CommandJobTask
            {
                Status = JobStatusType.Success
            });
            target.Status.Should().Be(JobStatusType.Success);


            target.Tasks.Add(new CommandJobTask
            {
                Status = JobStatusType.Started
            });
            target.Status.Should().Be(JobStatusType.Started);

            target.Tasks.Add(new CommandJobTask
            {
                Status = JobStatusType.Failure
            });
            target.Status.Should().Be(JobStatusType.Failure);
        }

        [Test]
        public void CreateSubtask_Test()
        {
            var target = new CommandJob();
            var result = target.CreateSubtask(CommandType.Ping, "ABC123", "DEF456");
            result.Id.Should().NotBeNullOrEmpty();
            result.Status.Should().Be(JobStatusType.Started);
            result.HandlingNodeId.Should().Be("DEF456");
            result.Target.Should().Be("ABC123");

            target.Tasks.Count.Should().Be(1);
        }

        [Test]
        public void MarkTaskComplete_Test()
        {
            var target = new CommandJob();
            var task = target.CreateSubtask(CommandType.Ping, "ABC", "123");
            task.Status.Should().Be(JobStatusType.Started);

            target.MarkTaskComplete("INVALID", true);
            task.Status.Should().Be(JobStatusType.Started);

            target.MarkTaskComplete(task.Id, true);
            task.Status.Should().Be(JobStatusType.Success);

            target.MarkTaskComplete(task.Id, false);
            task.Status.Should().Be(JobStatusType.Failure);
        }
    }
}
