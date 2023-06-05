using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

public interface ICommandHandler<TCommand, TResponse> : ICommandHandler
    where TCommand : class, ICommand<TResponse>
{
    ICommandResponse<TResponse> Execute(TCommand command);
    void Undo(TCommand command);
    void Redo(TCommand command);
}

public interface ICommandHandler
{
    ICommandResponse Execute(ICommand command);
    void Undo(ICommand command);
    void Redo(ICommand command);
}