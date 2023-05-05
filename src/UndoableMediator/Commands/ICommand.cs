using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

public interface ICommand
{
    /// <summary>
    ///     This will only be used through AddToSubCommands for when a command should be considered the parent of another.
    /// </summary>
    Stack<CommandBase> SubCommands { get; set; }

    /// <summary>
    ///     Allows you to set another command as the child of this one.
    /// </summary>
    /// <param name="command"></param>
    void AddToSubCommands(CommandBase command);
    void ExecuteBy(IUndoableMediator mediator, bool addToHistory = false);
}
