namespace UndoableMediator.Commands;

/// <summary>
///     Base generic interface for a command
/// </summary>
/// <typeparam name="TResponse"> The type of the response expected </typeparam>
public interface ICommand<TResponse> : ICommand
{
    /// <summary>
    ///     The stack of subCommands
    /// </summary>
    Stack<CommandBase> SubCommands { get; set; }

    /// <summary>
    ///     If the command triggers another command, register here to have a proper command tree.
    ///     This allows proper Undo/Redo propagation
    /// </summary>
    /// <param name="command"> The command to add to the subcommands </param>
    void AddToSubCommands(CommandBase command);
}

/// <summary>
///     The base interface for a command
/// </summary>
public interface ICommand
{
}

