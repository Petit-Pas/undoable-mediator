using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class SetRandomAgeCommandHandler : CommandHandlerBase<SetRandomAgeCommand, int>
{
    public override CommandResponse<int> Execute(SetRandomAgeCommand command, IUndoableMediator mediator)
    {
        var randomAgeQuery = new RandomIntQuery();
        var age = mediator.Execute(randomAgeQuery);
        var changeAgeCommand = new ChangeAgeCommand(age?.Response ?? 0);
        mediator.Execute(changeAgeCommand);
        command.AddToSubCommands(changeAgeCommand);

        return CommandResponse<int>.Success(age?.Response ?? 0);
    }
}
