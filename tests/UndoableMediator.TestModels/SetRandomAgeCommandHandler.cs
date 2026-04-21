using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class SetRandomAgeCommandHandler : CommandHandlerBase<SetRandomAgeCommand, int>
{
    public SetRandomAgeCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override async Task<ICommandResponse<int>> ExecuteAsync(SetRandomAgeCommand command)
    {
        var age = await _mediator.QueryAsync(new RandomIntQuery());
        await _mediator.SendAsSubCommandAsync(new ChangeAgeCommand(age?.Response ?? 0), command);
        return CommandResponse.Success(age?.Response ?? 0);
    }
}
