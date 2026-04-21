namespace UndoableMediator.Commands;

/// <summary>
///     Internal contract exposing the sub-command stack of a command.
///     Implemented by <see cref="CommandBase{TResponse}"/>. Not part of the public API.
/// </summary>
internal interface ISubCommandHost
{
    /// <summary>
    ///     The stack of sub-commands that have been dispatched under this command.
    ///     Iterated by <see cref="CommandHandlerBase{TCommand,TResponse}"/> to propagate undo and redo.
    /// </summary>
    Stack<ICommand> SubCommands { get; }

    /// <summary>
    ///     Registers a sub-command that was dispatched during this command's execution.
    ///     Called by the mediator after each <see cref=\"IUndoableMediator.SendAsSubCommandAsync{TSubResponse}\"/> call.
    /// </summary>
    /// <param name="command"> The sub-command to register </param>
    void AddSubCommand(ICommand command);

    /// <summary>
    ///     Clears all registered sub-commands.
    ///     Used when a handler needs to re-execute from scratch during redo instead of replaying the recorded sub-commands.
    /// </summary>
    void ClearSubCommands();
}
