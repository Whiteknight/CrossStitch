using CrossStitch.Core.Messages.Master;

namespace CrossStitch.Core.Modules.Master.Handlers
{
    public interface ICommandHandler
    {
        CommandResponse Handle(CommandRequest request);
        bool HandleLocal(CommandRequest request);
    }
}
