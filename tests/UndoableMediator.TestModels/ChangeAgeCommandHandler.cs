using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeAgeCommandHandler : CommandHandlerBase<ChangeAgeCommand>
{
    public ChangeAgeCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override Task<ICommandResponse<NoResponse>> ExecuteAsync(ChangeAgeCommand command)
    {
        command.OldAge = AffectedObject.Age;
        AffectedObject.Age = command.NewAge;

        return Task.FromResult(CommandResponse.Success());
    }

    public override async Task UndoAsync(ChangeAgeCommand command)
    {
        await base.UndoAsync(command);
        AffectedObject.Age = command.OldAge;
    }

    public override async Task RedoAsync(ChangeAgeCommand command)
    {
        await ExecuteAsync(command);
    }
}
