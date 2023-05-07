using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class SetRandomAgeCommandHandler : CommandHandlerBase<SetRandomAgeCommand>
{
    public override CommandResponse Execute(SetRandomAgeCommand command, IUndoableMediator mediator)
    {
        var randomAgeQuery = new RandomIntQuery();
        var age = randomAgeQuery.ExecuteBy(mediator);
        var changeAgeCommand = new ChangeAgeCommand(age?.Response ?? 0);
        mediator.Execute(changeAgeCommand);
        command.AddToSubCommands(changeAgeCommand);

        return CommandResponse.Success;
    }
}
