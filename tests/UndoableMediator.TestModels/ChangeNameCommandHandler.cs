using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeNameCommandHandler : CommandHandlerBase<ChangeNameCommand>
{
    public ChangeNameCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override Task<ICommandResponse<NoResponse>> ExecuteAsync(ChangeNameCommand command)
    {
        command.OldName = AffectedObject.Name;
        AffectedObject.Name = command.NewName;

        return Task.FromResult(CommandResponse.Success());
    }
    
    public override async Task UndoAsync(ChangeNameCommand command)
    {
        await base.UndoAsync(command);
        if (command.OldName != null)
        {
            AffectedObject.Name = command.OldName;
        }
    }

    public override async Task RedoAsync(ChangeNameCommand command)
    {
        await ExecuteAsync(command);
    }
}
