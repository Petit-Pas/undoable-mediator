using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

public abstract class CommonCommandHandlerBase<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    ///     This method gets called by the Undo() call to the CommandHandlerBase
    /// </summary>
    /// <param name="command"></param>
    /// <param name="mediator"></param>
    internal virtual void UndoSubCommands(TCommand command, IUndoableMediator mediator)
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
    // generic method
    public ICommandResponse Execute(ICommand command, IUndoableMediator mediator)
    {
        if (command is TCommand castedCommand)
        {
            return Execute(castedCommand, mediator);
        }
        throw new InvalidOperationException($"Cannot execute command of type {command.GetType().FullName} because it is not of type {typeof(TCommand).FullName}");
    }

    // specialized method, needs to be implemented
    public abstract ICommandResponse Execute(TCommand command, IUndoableMediator mediator);

    // generic method
    public void Undo(ICommand command, IUndoableMediator mediator)
    {
        if (command is TCommand castedCommand)
        {
            Undo(castedCommand, mediator);
        }
        else
        {
            throw new InvalidOperationException($"Cannot undo command of type {command.GetType().FullName} because it is not of type {typeof(TCommand).FullName}");
        }
    }

    // specialized method, can be overriden but don't forget to keep calling base.Undo() if the command has sub commands to undo them as well
    public virtual void Undo(TCommand command, IUndoableMediator mediator)
    {
        UndoSubCommands(command, mediator);
    }

}

public abstract class CommandHandlerBase<TCommand, TResponse> : CommonCommandHandlerBase<TCommand>, ICommandHandler<TCommand, TResponse>
    where TCommand : class, ICommand<TResponse>
{
    // generic method
    public ICommandResponse Execute(ICommand command, IUndoableMediator mediator)
    {
        if (command is TCommand castedCommand)
        {
            return Execute(castedCommand, mediator);
        }
        throw new InvalidOperationException($"Cannot execute command of type {command.GetType().FullName} because it is not of type {typeof(TCommand).FullName}");
    }

    // specialized method, needs to be implemented
    public abstract ICommandResponse<TResponse> Execute(TCommand command, IUndoableMediator mediator);

    // generic method
    public void Undo(ICommand command, IUndoableMediator mediator)
    {
        if (command is TCommand castedCommand)
        {
            Execute(castedCommand, mediator);
        }
        else
        {
            throw new InvalidOperationException($"Cannot undo command of type {command.GetType().FullName} because it is not of type {typeof(TCommand).FullName}");
        }
    }

    // specialized method, can be overriden but don't forget to keep calling base.Undo() if the command has sub commands to undo them as well
    public virtual void Undo(TCommand command, IUndoableMediator mediator)
    {
        UndoSubCommands(command, mediator);
    }
}

