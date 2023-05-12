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

    void UndoLastCommand();

    public static bool AddAlways(RequestStatus _) => true;
    public static bool AddNever(RequestStatus _) => false;
}
