using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

/// <summary>
///     Base class to use for a command that yields no response
/// </summary>
/// <typeparam name="TCommand"> The type of the command to handle </typeparam>
public abstract class CommandHandlerBase<TCommand> : CommandHandlerBase<TCommand, NoResponse>
    where TCommand : class, ICommand<NoResponse>
{
    public CommandHandlerBase(IUndoableMediator mediator) : base(mediator)
    {
    }
}

/// <summary>
///     Base class to use for a command that does yield a response
/// </summary>
/// <typeparam name="TCommand"> The type of the command </typeparam>
/// <typeparam name="TResponse"> The type of the response of the command </typeparam>
public abstract class CommandHandlerBase<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
    where TCommand : class, ICommand<TResponse>
{
    /// <summary>
    ///     The mediator instance, can be reused by any command to execute subcommands
    /// </summary>
    protected readonly IUndoableMediator _mediator;

    /// <summary>
    ///     Base constructor
    /// </summary>
    /// <param name="mediator"> A mediator instance, should be gotten through DI </param>
    public CommandHandlerBase(IUndoableMediator mediator)
    {
        _mediator = mediator;
    }

    // <inheritdoc />
    public ICommandResponse Execute(ICommand command)
    {
        if (command is not TCommand castedCommand)
        {
            throw new InvalidOperationException($"Cannot execute command of type {command.GetType().FullName} because it is not of type {typeof(TCommand).FullName}");
        }
        return Execute(castedCommand);
    }

    // <inheritdoc />
    public abstract ICommandResponse<TResponse> Execute(TCommand command);

    // <inheritdoc />
    public void Undo(ICommand command)
    {
        if (command is not TCommand castedCommand)
        {
            throw new InvalidOperationException($"Cannot undo command of type {command.GetType().FullName} because it is not of type {typeof(TCommand).FullName}");
        }
        Undo(castedCommand);
    }


    // <inheritdoc />
    public virtual void Undo(TCommand command)
    {
        UndoSubCommands(command);
    }

    /// <summary>
    ///     This method gets called by the Undo() call to the CommandHandlerBase
    ///     Will simply propagate the Undo call to the potential SubCommands registered
    /// </summary>
    /// <param name="command"> The command to undo </param>
    private void UndoSubCommands(TCommand command)
    {
        foreach (var subCommand in command.SubCommands)
        {
            _mediator.Undo(subCommand);
        }
    }

    // <inheritdoc />
    public void Redo(ICommand command)
    {
        if (command is not TCommand castedCommand)
        {
            throw new InvalidOperationException($"Cannot undo command of type {command.GetType().FullName} because it is not of type {typeof(TCommand).FullName}");
        }
        Redo(castedCommand);
    }

    // <inheritdoc />
    public virtual void Redo(TCommand command)
    {
        RedoSubCommands(command);
    }

    /// <summary>
    ///     Will simply propagate the redo call to the porential subCommands registered.
    /// </summary>
    /// <param name="command"></param>
    private void RedoSubCommands(TCommand command)
    {
        foreach (var subCommand in command.SubCommands)
        {
            _mediator.Redo(subCommand);
        }
    }
}

