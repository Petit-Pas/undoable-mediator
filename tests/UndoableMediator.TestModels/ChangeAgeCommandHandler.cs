using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeAgeCommandHandler : CommandHandlerBase<ChangeAgeCommand>
{
    public ChangeAgeCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override CommandResponse Execute(ChangeAgeCommand command)
    {
        command.OldAge = AffectedObject.Age;
        AffectedObject.Age = command.NewAge;

        return CommandResponse.Success();
    }

    public override void Undo(ChangeAgeCommand command)
    {
        base.Undo(command);
        AffectedObject.Age = command.OldAge;
    }
}
