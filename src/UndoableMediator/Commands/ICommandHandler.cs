namespace UndoableMediator.Commands;

/// <summary>
///     Base generic interface for command handlers.
/// </summary>
/// <typeparam name="TCommand"> The type of the command to handle </typeparam>
/// <typeparam name="TResponse"> The type of the response given by the command </typeparam>
public interface ICommandHandler<TCommand, TResponse> : ICommandHandler
    where TCommand : class, ICommand<TResponse>
{
    /// <summary>
    ///     This is the actual Execute method that does the job, should be implemented by the handler implementation
    /// </summary>
    /// <param name="command"> The command to execute </param>
    /// <returns> The command response </returns>
    Task<ICommandResponse<TResponse>> ExecuteAsync(TCommand command);

    /// <summary>
    ///     Undoes the command. The default implementation in <see cref="CommandHandlerBase{TCommand,TResponse}"/>
    ///     propagates undo to all sub-commands in reverse execution order.
    ///     <para>
    ///         If you override this method, you <b>must</b> either call <c>base.UndoAsync(command)</c>
    ///         to preserve automatic sub-command propagation, or handle sub-commands manually.
    ///     </para>
    /// </summary>
    /// <param name="command"> The command to undo </param>
    Task UndoAsync(TCommand command);
    
    /// <summary>
    ///     Redoes the command. The default implementation in <see cref="CommandHandlerBase{TCommand,TResponse}"/>
    ///     propagates redo to all sub-commands in original execution order.
    ///     <para>
    ///         If you override this method, you <b>must</b> either call <c>base.RedoAsync(command)</c>
    ///         to preserve automatic sub-command propagation, or handle sub-commands manually.
    ///         You may also call <c>ClearSubCommands(command)</c> followed by <c>ExecuteAsync(command)</c>
    ///         to re-execute from scratch instead of replaying recorded sub-commands.
    ///     </para>
    /// </summary>
    /// <param name="command"> The command to redo </param>
    Task RedoAsync(TCommand command);
}

/// <summary>
///     Base interface for command handlers.
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    ///     Generic method, will be called first, then specialize into the typed overload.
    /// </summary>
    /// <param name="command"> The command to execute </param>
    /// <returns> The response of the command, can be cast to ICommandResponse&lt;TResponse&gt; </returns>
    /// <exception cref="InvalidOperationException"> Should never be thrown, unless being called wrongly by something else than the mediator </exception>
    Task<ICommandResponse> ExecuteAsync(ICommand command);

    /// <summary>
    ///     Generic method, will be called first, then specialize into the typed overload.
    /// </summary>
    /// <param name="command"> The command to undo </param>
    /// <exception cref="InvalidOperationException"> Should never be thrown, unless being called wrongly by something else than the mediator </exception>
    Task UndoAsync(ICommand command);

    /// <summary>
    ///     Generic method, will be called first, then specialize into the typed overload.
    /// </summary>
    /// <param name="command"> The command to redo </param>
    /// <exception cref="InvalidOperationException"> Should never be thrown, unless being called wrongly by something else than the mediator </exception>
    Task RedoAsync(ICommand command);
}