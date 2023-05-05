using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeNameCommandHandler : CommandHandlerBase<ChangeNameCommand>
{
    public static void Execute(ChangeNameCommand command)
    {
        command.OldName = AffectedObject.Name;
        AffectedObject.Name = command.NewName;
    }
    
    public override void Undo(ChangeNameCommand command, IUndoableMediator mediator)
    {
        if (command.OldName != null)
        {
            AffectedObject.Name = command.OldName;
        }
    }
}
