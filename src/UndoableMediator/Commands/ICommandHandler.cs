using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

public interface ICommandHandler<T>
{
    void Undo(T command, IUndoableMediator mediator);
    CommandResponse Execute(T command, IUndoableMediator mediator);
}
