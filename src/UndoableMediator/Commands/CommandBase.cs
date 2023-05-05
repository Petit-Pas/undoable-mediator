using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

public class CommandBase : ICommand
{
    public void ExecuteBy(IUndoableMediator mediator, bool addToHistory = false)
    {
        mediator.Execute(this, addToHistory);
    }

    public void AddToSubCommands(CommandBase command)
    {
        SubCommands.Push(command);
    }

    public Stack<CommandBase> SubCommands { get; set; } = new Stack<CommandBase>();
}
