namespace UndoableMediator.Commands;

public abstract class CommandBase : CommandBase<NoResponse>
{
}

public abstract class CommandBase<TResponse> : ICommand<TResponse>
{
    public void AddToSubCommands(CommandBase command)
    {
        SubCommands.Push(command);
    }

    public Stack<CommandBase> SubCommands { get; set; } = new ();
}
