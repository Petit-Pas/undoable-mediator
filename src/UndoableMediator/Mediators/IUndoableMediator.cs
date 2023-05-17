using UndoableMediator.Commands;
using UndoableMediator.Queries;
using UndoableMediator.Requests;

namespace UndoableMediator.Mediators;

public interface IUndoableMediator
{
    ICommandResponse Execute(ICommand command, Func<RequestStatus, bool>? shouldAddCommandToHistory = null);

    ICommandResponse<TResponse>? Execute<TResponse>(ICommand<TResponse> command, Func<RequestStatus, bool>? shouldAddCommandToHistory = null);

    IQueryResponse<T>? Execute<T>(IQuery<T> query);

    void Undo(ICommand command);

    /// <summary>
    /// 
    /// </summary>
    /// <returns> true if there was a command to undo, false otherwise </returns>
    bool UndoLastCommand();

    public int HistoryLength { get; }

    public static bool AddAlways(RequestStatus _) => true;
    public static bool AddNever(RequestStatus _) => false;
}
