using UndoableMediator.Commands;
using UndoableMediator.Queries;

namespace UndoableMediator.Mediators;

/// <remarks>
///     <b>Thread safety:</b> This mediator is designed for single-user (e.g. desktop) scenarios.
///     The command history is not thread-safe. If you need concurrent access, implement your own synchronisation.
/// </remarks>
public interface IUndoableMediator
{
    /// <summary>
    ///     Sends a command to be executed by its handler.
    ///     If the handler returns <see cref="Requests.RequestStatus.Success"/>, the command is automatically added to the undo history.
    /// </summary>
    /// <typeparam name="TResponse"> The response type expected from the command </typeparam>
    /// <param name="command"> The command to execute </param>
    /// <returns> The CommandResponse expected by the command </returns>
    Task<ICommandResponse<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command);

    /// <summary>
    ///     Sends a command as a sub-command of a parent command.
    ///     The sub-command is executed and automatically registered in the parent's sub-command tree,
    ///     enabling cascading undo/redo when the parent is undone or redone.
    /// </summary>
    /// <typeparam name="TSubResponse"> The response type of the sub-command </typeparam>
    /// <param name="subCommand"> The sub-command to execute </param>
    /// <param name="parentCommand"> The parent command that owns this sub-command </param>
    /// <returns> The response of the sub-command </returns>
    Task<ICommandResponse<TSubResponse>> SendAsSubCommandAsync<TSubResponse>(ICommand<TSubResponse> subCommand, ICommand parentCommand);

    /// <summary>
    ///     Sends a query to be executed by its handler.
    /// </summary>
    /// <typeparam name="TResponse"> The type of the response expected by this query </typeparam>
    /// <param name="query"> The query to execute </param>
    /// <returns> The QueryResponse expected by the query </returns>
    Task<IQueryResponse<TResponse>> QueryAsync<TResponse>(IQuery<TResponse> query);

    /// <summary>
    ///     Takes the last executed command from the history and undoes it.
    /// </summary>
    /// <returns> true if there was a command to undo, false otherwise </returns>
    Task<bool> UndoLastCommandAsync();

    /// <summary>
    ///     If a command was previously undone with UndoLastCommandAsync, and none has been added to the history since,
    ///     this will re-execute the last undone command and push it back to the history stack.
    ///     
    ///     For this to work well on a command that needs to get data (for instance a command that queries a value, then updates a model accordingly),
    ///     it is very important that the handler knows if it needs to re-query the data, or store the result of the query in the command for later usage.
    /// </summary>
    /// <returns> true if there was a command to redo, false otherwise </returns>
    Task<bool> RedoLastUndoneCommandAsync();

    /// <summary>
    ///     Property to access the inner length of the history.
    ///     Once that property reaches the maximum provided in the DI configuration, the oldest commands will be discarded one by one.
    /// </summary>
    public int HistoryLength { get; }

    /// <summary>
    ///     Property to access the inner length of the undone commands, they can be re-executed through RedoLastUndoneCommandAsync.
    /// </summary>
    public int RedoHistoryLength { get; }

    /// <summary>
    ///     Fired after a top-level command (sent via <see cref="SendAsync{TResponse}"/>) is successfully executed and added to the undo history.
    ///     Not fired for sub-commands dispatched via <see cref="SendAsSubCommandAsync{TSubResponse}"/>.
    /// </summary>
    event EventHandler<ICommand>? OnCommandExecuted;

    /// <summary>
    ///     Fired after <see cref="UndoLastCommandAsync"/> successfully undoes a command.
    ///     The event argument is the command that was undone.
    /// </summary>
    event EventHandler<ICommand>? OnCommandUndone;

    /// <summary>
    ///     Fired after <see cref="RedoLastUndoneCommandAsync"/> successfully redoes a command.
    ///     The event argument is the command that was redone.
    /// </summary>
    event EventHandler<ICommand>? OnCommandRedone;

}
