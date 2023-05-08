using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeNameCommandHandler : CommandHandlerBase<ChangeNameCommand>
{
    public override CommandResponse Execute(ChangeNameCommand command, IUndoableMediator mediator)
    {
        command.OldName = AffectedObject.Name;
        AffectedObject.Name = command.NewName;

        return CommandResponse.Success();
    }
    
    public override void Undo(ChangeNameCommand command, IUndoableMediator mediator)
    {
        if (command.OldName != null)
        {
            AffectedObject.Name = command.OldName;
        }
    }
}
