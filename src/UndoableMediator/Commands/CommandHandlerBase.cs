using UndoableMediator.Mediators;
using UndoableMediator.Requests;

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
    ///     The mediator instance, can be reused by any command to execute sub-commands
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

    // ──────────────────────────────────────────────
    //  Execute / Undo / Redo — override these in your handler
    // ──────────────────────────────────────────────

    /// <inheritdoc />
    public abstract Task<ICommandResponse<TResponse>> ExecuteAsync(TCommand command);

    /// <summary>
    ///     Undoes the command. By default, propagates undo to all sub-commands in reverse execution order.
    ///     <para>
    ///         Override this to restore state mutated in <see cref="ExecuteAsync(TCommand)"/>.
    ///         You <b>must</b> call <c>base.UndoAsync(command)</c> (typically before your own logic)
    ///         to propagate undo to sub-commands, or handle them manually.
    ///     </para>
    /// </summary>
    public virtual async Task UndoAsync(TCommand command)
    {
        await PropagateToSubCommandsAsync(command, SubCommandOperation.Undo);
    }

    /// <summary>
    ///     Redoes the command. By default, propagates redo to all sub-commands in original execution order.
    ///     <para>
    ///         Override this to re-apply state. You <b>must</b> either call <c>base.RedoAsync(command)</c>
    ///         to propagate redo to sub-commands, or handle them manually.
    ///         Alternatively, call <c>ClearSubCommands(command)</c> followed by <c>ExecuteAsync(command)</c>
    ///         to re-execute the command from scratch.
    ///     </para>
    /// </summary>
    public virtual async Task RedoAsync(TCommand command)
    {
        await PropagateToSubCommandsAsync(command, SubCommandOperation.Redo);
    }

    // ──────────────────────────────────────────────
    //  Sub-command utilities — available to handler implementations
    // ──────────────────────────────────────────────

    /// <summary>
    ///     Clears all sub-commands previously registered on the command.
    ///     Call this in <see cref="RedoAsync(TCommand)"/> before re-executing the command
    ///     when stale sub-commands should be discarded and re-created from scratch.
    /// </summary>
    /// <param name="command"> The command whose sub-command stack should be cleared </param>
    protected void ClearSubCommands(TCommand command)
    {
        if (command is not ISubCommandHost host)
        {
            throw new InvalidOperationException($"Command of type {command.GetType().FullName} does not implement {nameof(ISubCommandHost)}. Only commands deriving from {nameof(CommandBase)} support sub-commands.");
        }
        host.ClearSubCommands();
    }

    // ──────────────────────────────────────────────
    //  Non-generic dispatch — called by the Mediator,
    //  casts to TCommand then delegates to the typed overload above
    // ──────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<ICommandResponse> ExecuteAsync(ICommand command)
    {
        var castedCommand = RequestCastHelper.CastOrThrow<TCommand>(command, "execute");
        return await ExecuteAsync(castedCommand);
    }

    /// <inheritdoc />
    public async Task UndoAsync(ICommand command)
    {
        var castedCommand = RequestCastHelper.CastOrThrow<TCommand>(command, "undo");
        await UndoAsync(castedCommand);
    }

    /// <inheritdoc />
    public async Task RedoAsync(ICommand command)
    {
        var castedCommand = RequestCastHelper.CastOrThrow<TCommand>(command, "redo");
        await RedoAsync(castedCommand);
    }

    // ──────────────────────────────────────────────
    //  Sub-command propagation
    // ──────────────────────────────────────────────

    private enum SubCommandOperation { Undo, Redo }

    /// <summary>
    ///     Propagates the given operation to all sub-commands.
    ///     <list type="bullet">
    ///         <item><description>Undo: iterates in reverse execution order (LIFO — last executed first).</description></item>
    ///         <item><description>Redo: iterates in original execution order (FIFO — first executed first).</description></item>
    ///     </list>
    /// </summary>
    private async Task PropagateToSubCommandsAsync(TCommand command, SubCommandOperation operation)
    {
        if (command is not ISubCommandHost host)
        {
            return;
        }

        var dispatcher = _mediator as ISubCommandDispatcher 
            ?? throw new InvalidOperationException($"The mediator must implement {nameof(ISubCommandDispatcher)} to support sub-command {operation.ToString().ToLowerInvariant()}.");

        // Stack iterates LIFO naturally — correct for undo.
        // Reversing gives FIFO — correct for redo.
        var subCommands = operation == SubCommandOperation.Undo
            ? host.SubCommands
            : host.SubCommands.Reverse();

        foreach (var subCommand in subCommands)
        {
            switch (operation)
            {
                case SubCommandOperation.Undo:
                    await dispatcher.UndoAsync(subCommand);
                    break;
                case SubCommandOperation.Redo:
                    await dispatcher.RedoAsync(subCommand);
                    break;
            }
        }
    }
}
