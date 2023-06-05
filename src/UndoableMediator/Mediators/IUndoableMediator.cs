using UndoableMediator.Commands;
using UndoableMediator.Queries;
using UndoableMediator.Requests;

namespace UndoableMediator.Mediators;

public interface IUndoableMediator
{
    /// <summary>
    ///     Can be used as parameter for Execute(ICommand) method, to add to history no matter what happens with the status
    /// </summary>
    public static bool AddAlways(RequestStatus _) => true;

    /// <summary>
    ///     The method to execute a command
    /// </summary>
    /// <typeparam name="TResponse"> The response type expected from the command </typeparam>
    /// <param name="command"> The command to execute </param>
    /// <param name="shouldAddCommandToHistory"> A delegate to tell if the command should be added to the history or not, depending on the RequestStatus </param>
    /// <returns> The CommandResponse expected by the command </returns>
    ICommandResponse<TResponse> Execute<TResponse>(ICommand<TResponse> command, Func<RequestStatus, bool>? shouldAddCommandToHistory = null);

    /// <summary>
    ///     The method to execute a query
    /// </summary>
    /// <typeparam name="TResponse"> The type of the response expected by this query </typeparam>
    /// <param name="query"> The query to execute </param>
    /// <returns> The QueryResponse expected by the query </returns>
    IQueryResponse<TResponse> Execute<TResponse>(IQuery<TResponse> query);

    /// <summary>
    ///     A method to Undo a command. 
    ///     This method should normally not be called by client code, it will be used internally by UndoLastCommand
    /// </summary>
    /// <param name="command"> The command to undo </param>
    void Undo(ICommand command);

    /// <summary>
    ///     This is the method that will take the last Command execute it, and call Undo upon it.
    /// </summary>
    /// <returns> true if there was a command to undo, false otherwise </returns>
    bool UndoLastCommand();

    /// <summary>
    ///     A method to redo a command previously undone.
    ///     This method should normally not be called by client code, it will be used internally by RedoLastUndoneCommand
    /// </summary>
    /// <param name="command"></param>
    void Redo(ICommand command);

    /// <summary>
    ///     If a command was previously undone with UndoLastCommand, and none has been added added to the history since
    ///     this will re Execute the last undone command and push it back to the history stack.
    ///     
    ///     For this to work well on a command that needs to get data (for instance a command that queries a value, then update a model accordingly)
    ///     It is very important that the handler knows if it needs to requery the data, or store the result of the query in the command for later usage.
    /// </summary>
    /// <returns> true if there was a command to redo, false otherwise </returns>
    bool RedoLastUndoneCommand();

    /// <summary>
    ///     Property to access the inner length of the history.
    ///     Once that property reaches the maximum provided in the DI configuration, oldest ones will be discoarded one by one.
    /// </summary>
    public int HistoryLength { get; }

    /// <summary>
    ///     Property to access the inner length of the undone commands, they can be re executed through RedoLastUndoneCommand
    /// </summary>
    public int RedoHistoryLength { get; }

}
