using UndoableMediator.Commands;
using UndoableMediator.Queries;
using UndoableMediator.Requests;

namespace UndoableMediator.Mediators;

public interface IUndoableMediator
{
    ICommandResponse<TResponse> Execute<TResponse>(ICommand<TResponse> command, Func<RequestStatus, bool>? shouldAddCommandToHistory = null);

    IQueryResponse<TResponse> Execute<TResponse>(IQuery<TResponse> query);

    void Undo(ICommand command);

    /// <summary>
    /// 
    /// </summary>
    /// <returns> true if there was a command to undo, false otherwise </returns>
    bool UndoLastCommand();

    /// <summary>
    ///     If a command was previously undone with UndoLastCommand, and none has been added added to the history since
    ///     this will re Execute the last undone command and push it back to the history stack.
    ///     
    ///     For this to work well on a command that needs to get data (for instance a command that queries a value, then update a model accordingly)
    ///     It is very important that the handler knows if it needs to requery the data, or store the result of the query in the command for later usage.
    /// </summary>
    /// <returns></returns>
    bool RedoLastUndoneCommand();

    void Redo(ICommand command);

    public int HistoryLength { get; }

    public int RedoHistoryLength { get; }

    public static bool AddAlways(RequestStatus _) => true;
    public static bool AddNever(RequestStatus _) => false;
}
