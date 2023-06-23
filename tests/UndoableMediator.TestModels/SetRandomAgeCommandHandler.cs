using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class SetRandomAgeCommandHandler : CommandHandlerBase<SetRandomAgeCommand, int>
{
    public SetRandomAgeCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override async Task<ICommandResponse<int>> Execute(SetRandomAgeCommand command)
    {
        var randomAgeQuery = new RandomIntQuery();
        var age = await _mediator.Execute(randomAgeQuery);
        var changeAgeCommand = new ChangeAgeCommand(age?.Response ?? 0);
        await _mediator.Execute(changeAgeCommand);
        command.AddToSubCommands(changeAgeCommand);

        return CommandResponse.Success(age?.Response ?? 0);
    }
}
