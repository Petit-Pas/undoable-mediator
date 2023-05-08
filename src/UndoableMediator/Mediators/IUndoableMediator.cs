using UndoableMediator.Commands;
using UndoableMediator.Queries;

namespace UndoableMediator.Mediators;

public interface IUndoableMediator
{
    void Execute(ICommand command, bool addToHistory = false);

    IQueryResponse<T>? Execute<T>(IQuery<T> query);

    void Undo(ICommand command);
}
