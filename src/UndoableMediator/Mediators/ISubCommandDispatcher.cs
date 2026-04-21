using UndoableMediator.Commands;

namespace UndoableMediator.Mediators;

/// <summary>
///     Internal contract used by <see cref="UndoableMediator.Commands.CommandHandlerBase{TCommand,TResponse}"/>
///     to propagate undo and redo operations to sub-commands.
///     Not part of the public API — do not depend on this interface in application code.
/// </summary>
internal interface ISubCommandDispatcher
{
    /// <summary>
    ///     Undoes the given command by dispatching to its handler.
    /// </summary>
    /// <param name="command"> The command to undo </param>
    Task UndoAsync(ICommand command);

    /// <summary>
    ///     Redoes the given command by dispatching to its handler.
    /// </summary>
    /// <param name="command"> The command to redo </param>
    Task RedoAsync(ICommand command);
}
