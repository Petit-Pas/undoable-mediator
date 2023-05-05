using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeAgeCommandHandler : CommandHandlerBase<ChangeAgeCommand>
{
    public static void Execute(ChangeAgeCommand command)
    {
        command.OldAge = AffectedObject.Age;
        AffectedObject.Age = command.NewAge;
    }

    public override void Undo(ChangeAgeCommand command, IUndoableMediator mediator)
    {
        AffectedObject.Age = command.OldAge;
    }
}
