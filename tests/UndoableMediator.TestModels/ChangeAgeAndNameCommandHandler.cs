using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeAgeAndNameCommandHandler : CommandHandlerBase<ChangeAgeAndNameCommand>
{
    public override CommandResponse Execute(ChangeAgeAndNameCommand command, IUndoableMediator mediator)
    {
        var changeAgeCmd = new ChangeAgeCommand(command.Age);
        var changeNameCmd = new ChangeNameCommand(command.Name);

        mediator.Execute(changeAgeCmd);
        command.AddToSubCommands(changeAgeCmd);

        mediator.Execute(changeNameCmd);
        command.AddToSubCommands(changeNameCmd);

        return CommandResponse.Success();
    }
}
