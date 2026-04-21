namespace UndoableMediator.Commands;

/// <summary>
///     The base class to use for a command that returns nothing.
/// </summary>
public abstract class CommandBase : CommandBase<NoResponse>
{
}

/// <summary>
///     The base class to use for a command that returns something.
/// </summary>
/// <typeparam name="TResponse"> The type of the response from the command </typeparam>
public abstract class CommandBase<TResponse> : ICommand<TResponse>, ISubCommandHost
{
    private readonly Stack<ICommand> _subCommands = new();

    Stack<ICommand> ISubCommandHost.SubCommands
    {
        get { return _subCommands; }
    }

    void ISubCommandHost.AddSubCommand(ICommand command)
    {
        _subCommands.Push(command);
    }

    void ISubCommandHost.ClearSubCommands()
    {
        _subCommands.Clear();
    }
}