using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeAgeCommandHandler : CommandHandlerBase<ChangeAgeCommand>
{
    public override CommandResponse Execute(ChangeAgeCommand command, IUndoableMediator _)
    {
        command.OldAge = AffectedObject.Age;
        AffectedObject.Age = command.NewAge;

        return CommandResponse.Success();
    }

    public override void Undo(ChangeAgeCommand command, IUndoableMediator mediator)
    {
        AffectedObject.Age = command.OldAge;
    }
}
