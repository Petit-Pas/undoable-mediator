using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeAgeAndNameCommandHandler
{
    public static void Execute(ChangeAgeAndNameCommand command, IUndoableMediator mediator)
    {
        var changeAgeCmd = new ChangeAgeCommand(command.Age);
        var changeNameCmd = new ChangeNameCommand(command.Name);

        changeAgeCmd.ExecuteBy(mediator);
        command.AddToSubCommands(changeAgeCmd);

        changeNameCmd.ExecuteBy(mediator);
        command.AddToSubCommands(changeNameCmd);
    }
}
