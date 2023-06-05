using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

public abstract class CommandHandlerBase<TCommand> : CommandHandlerBase<TCommand, NoResponse>
    where TCommand : class, ICommand<NoResponse>
{
    public CommandHandlerBase(IUndoableMediator mediator) : base(mediator)
    {
    }
}

public abstract class CommandHandlerBase<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : class, ICommand<TResponse>
{
    protected readonly IUndoableMediator _mediator;

    public CommandHandlerBase(IUndoableMediator mediator)
    {
        _mediator = mediator;
    }

    //// generic method
    public ICommandResponse Execute(ICommand command)
    {
        if (command is not TCommand castedCommand)
        {
            throw new InvalidOperationException($"Cannot execute command of type {command.GetType().FullName} because it is not of type {typeof(TCommand).FullName}");
        }
        return Execute(castedCommand);
    }

    //// specialized method, needs to be implemented
    public abstract ICommandResponse<TResponse> Execute(TCommand command);

    // generic method
    public void Undo(ICommand command)
    {
        if (command is not TCommand castedCommand)
        {
            throw new InvalidOperationException($"Cannot undo command of type {command.GetType().FullName} because it is not of type {typeof(TCommand).FullName}");
        }
        Undo(castedCommand);
    }

    // specialized method, can be overriden but don't forget to keep calling base.Undo() if the command has sub commands to undo them as well
    public virtual void Undo(TCommand command)
    {
        UndoSubCommands(command);
    }

    /// <summary>
    ///     This method gets called by the Undo() call to the CommandHandlerBase
    /// </summary>
    /// <param name="command"></param>
    /// <param name="mediator"></param>
    private void UndoSubCommands(TCommand command)
    {
        foreach (var subCommand in command.SubCommands)
        {
            _mediator.Undo(subCommand);
        }
    }

    public void Redo(ICommand command)
    {
        if (command is not TCommand castedCommand)
        {
            throw new InvalidOperationException($"Cannot undo command of type {command.GetType().FullName} because it is not of type {typeof(TCommand).FullName}");
        }
        Redo(castedCommand);
    }

    public virtual void Redo(TCommand command)
    {
        RedoSubCommands(command);
    }

    private void RedoSubCommands(TCommand command)
    {
        foreach (var subCommand in command.SubCommands)
        {
            _mediator.Redo(subCommand);
        }
    }
}

