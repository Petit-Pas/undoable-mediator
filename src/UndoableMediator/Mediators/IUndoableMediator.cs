using UndoableMediator.Commands;
using UndoableMediator.Queries;

namespace UndoableMediator.Mediators;

public interface IUndoableMediator
{
    void Execute(ICommand command, bool AddToHistory = false);

    IQueryResponse<T> Execute<T>(IQuery query);

    void Undo(ICommand command);
}
