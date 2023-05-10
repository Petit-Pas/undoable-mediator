using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

public abstract class CommonCommandHandlerBase<TCommand>
    where TCommand : ICommand
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
    public virtual void Undo(TCommand command, IUndoableMediator mediator)
    {
        // maybe this can be set in the base class instead
        foreach (var subCommand in command.SubCommands)
        {
            mediator.Undo(subCommand);
        }
    }
}

public abstract class CommandHandlerBase<TCommand> : CommonCommandHandlerBase<TCommand>, ICommandHandler<TCommand>
    where TCommand : class, ICommand
{
    public abstract ICommandResponse Execute(TCommand command, IUndoableMediator mediator);

    public void GenericUndo(ICommand command, IUndoableMediator mediator)
    {
        Undo(command as TCommand, mediator);
    }

    public ICommandResponse Execute(ICommand command, IUndoableMediator mediator)
    {
        return this.Execute(command as TCommand, mediator);
    }
}

public abstract class CommandHandlerBase<TCommand, TResponse> : CommonCommandHandlerBase<TCommand>, ICommandHandler<TCommand, TResponse>
    where TCommand : class, ICommand<TResponse>
{
    public abstract ICommandResponse<TResponse> Execute(TCommand command, IUndoableMediator mediator);

    public void GenericUndo(ICommand command, IUndoableMediator mediator)
    {
        Undo(command as TCommand, mediator);
    }

    public ICommandResponse Execute(ICommand command, IUndoableMediator mediator)
    {
        return Execute(command as TCommand, mediator);
    }
}

