using UndoableMediator.Mediators;

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

    /// <summary>
    ///     If the command triggers another command, trigger it with this method, which will register it in the subCommands
    ///     This allows proper Undo/Redo propagation
    /// </summary>
    /// <param name="mediator"> the mediator instance to execute the command </param>
    /// <param name="command"> The command to add to the subcommands </param>
    void ExecuteSubCommand(IUndoableMediator mediator, CommandBase command);
}

/// <summary>
///     The base interface for a command
/// </summary>
public interface ICommand
{
}

