using CrossStitch.Core.Messages.Master;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Core.Models
{
    public enum JobStatusType
    {
        Started,
        Complete,
        Failure
    }

    public class CommandJob : IDataEntity
    {
        public CommandJob()
        {
            Tasks = new List<CommandJobTask>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public long StoreVersion { get; set; }

        public List<CommandJobTask> Tasks { get; }

        public JobStatusType Status
        {
            get
            {
                if (Tasks.Any(t => t.Status == JobStatusType.Failure))
                    return JobStatusType.Failure;
                if (Tasks.Any(t => t.Status == JobStatusType.Started))
                    return JobStatusType.Started;
                return JobStatusType.Complete;
            }
        }

        public CommandJobTask CreateSubtask(CommandType command, string target, string handlingNodeId)
        {
            var task = new CommandJobTask
            {
                Id = Guid.NewGuid().ToString(),
                Status = JobStatusType.Started,
                HandlingNodeId = handlingNodeId,
                Command = command,
                Target = target
            };
            Tasks.Add(task);
            return task;
        }

        public void MarkTaskComplete(string taskId, bool success)
        {
            var task = Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                return;
            task.Status = success ? JobStatusType.Complete : JobStatusType.Failure;
        }
    }

    public class CommandJobTask
    {
        public string Id { get; set; }
        public JobStatusType Status { get; set; }
        public string HandlingNodeId { get; set; }
        public CommandType Command { get; set; }
        public string Target { get; set; }

        // TODO: Include enough information here that we could retry the task if it is hung or
        // failed.
    }
}
