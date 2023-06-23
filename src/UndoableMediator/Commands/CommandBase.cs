using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

/// <summary>
///     The base class to use for a command that returns nothing.
/// </summary>
public abstract class CommandBase : CommandBase<NoResponse>
{
}

/// <summary>
///     The base class to use for a command that returns something
/// </summary>
/// <typeparam name="TResponse"> The type of the response from the command </typeparam>
public abstract class CommandBase<TResponse> : ICommand<TResponse>
{
    // <inheritdoc />
    public void AddToSubCommands(CommandBase command)
    {
        SubCommands.Push(command);
    }

    public void ExecuteSubCommand(IUndoableMediator mediator, CommandBase command)
    {
        mediator.Execute(command);
        AddToSubCommands(command);
    }

    // <inheritdoc />
    public Stack<CommandBase> SubCommands { get; set; } = new ();
}
