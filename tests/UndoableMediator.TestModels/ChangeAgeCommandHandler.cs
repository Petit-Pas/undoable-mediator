using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeAgeCommandHandler : CommandHandlerBase<ChangeAgeCommand>
{
    public ChangeAgeCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override Task<ICommandResponse<NoResponse>> Execute(ChangeAgeCommand command)
    {
        command.OldAge = AffectedObject.Age;
        AffectedObject.Age = command.NewAge;

        return Task.FromResult(CommandResponse.Success());
    }

    public override void Undo(ChangeAgeCommand command)
    {
        base.Undo(command);
        AffectedObject.Age = command.OldAge;
    }

    public override void Redo(ChangeAgeCommand command)
    {
        Execute(command);
    }
}
