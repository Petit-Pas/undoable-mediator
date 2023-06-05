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
    ICommandResponse<TResponse> Execute(TCommand command);

    /// <summary>
    ///     This is the actual Undo method that does the job, by default it only propagates to subcommands.
    ///     Can be overridden to add some more comportments, should be the case if the command actually modified the state of anything.
    /// </summary>
    /// <param name="command"> The command to undo </param>
    void Undo(TCommand command);
    
    /// <summary>
    ///     This is the actual Redo method that does the job, by default it only propagates to subcommands.
    ///     Can be overridden to add some more comportments, should be the case if the command actually modifies the state of anything.
    /// </summary>
    /// <param name="command"> The command to redo </param>
    void Redo(TCommand command);
}

/// <summary>
///     Base interface for command handlers.
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    ///     Generic method, will be called first, then specialize into the other "Execute()"
    /// </summary>
    /// <param name="command"> The command to execute </param>
    /// <returns> The response of the command, can be casted into ICommandResponse<TResponse> </returns>
    /// <exception cref="InvalidOperationException"> Should never be thrown, unless being called wrongly by something else than the mediator </exception>
    ICommandResponse Execute(ICommand command);

    /// <summary>
    ///     Generic method, will be called first, then specialize into the other "Undo()"
    /// </summary>
    /// <param name="command"> The command to undo </param>
    /// <exception cref="InvalidOperationException"> Should never be thrown, unless being called wrongly by something else than the mediator </exception>
    void Undo(ICommand command);

    /// <summary>
    ///     Generic method, will be called first, then specialize into the other "Redo()"
    /// </summary>
    /// <param name="command"> The command to redo </param>
    /// <exception cref="InvalidOperationException"> Should never be thrown, unless being called wrongly by something else than the mediator </exception>
    void Redo(ICommand command);
}