using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

public abstract class CommandHandlerBase<T> : ICommandHandler<T>
    where T : ICommand
{
    /// <summary>
    ///     Override this method when you command handler actually modifies anything. 
    ///     If the handler is only sending other commands, then that method is sufficient.
    ///     
    ///     Side note: Ideally, a command should either send other ones, or modify something, never both.
    ///         So technically, if you were to override this method, you can probably avoid the base call.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="mediator"></param>
    public virtual void Undo(T command, IUndoableMediator mediator)
    {
        // maybe this can be set in the base class instead
        foreach (var subCommand in command.SubCommands)
        {
            mediator.Undo(subCommand);
        }
    }

    public abstract CommandResponse Execute(T command, IUndoableMediator mediator);
}
