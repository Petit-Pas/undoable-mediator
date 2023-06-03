using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class SetRandomAgeCommandHandler : CommandHandlerBase<SetRandomAgeCommand, int>
{
    public SetRandomAgeCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override CommandResponse<int> Execute(SetRandomAgeCommand command)
    {
        var randomAgeQuery = new RandomIntQuery();
        var age = _mediator.Execute<RandomIntQuery, int>(randomAgeQuery);
        var changeAgeCommand = new ChangeAgeCommand(age?.Response ?? 0);
        _mediator.Execute(changeAgeCommand);
        command.AddToSubCommands(changeAgeCommand);

        return CommandResponse<int>.Success(age?.Response ?? 0);
    }
}
