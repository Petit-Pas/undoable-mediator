using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

public interface ICommandHandler<TCommand> : ICommandHandler
    where TCommand : class, ICommand
{
    void Undo(TCommand command, IUndoableMediator mediator);
    CommandResponse Execute(TCommand command, IUndoableMediator mediator);
}

public interface ICommandHandler<TCommand, TResponse> : ICommandHandler
    where TCommand : class, ICommand<TResponse>
{
    void Undo(TCommand command, IUndoableMediator mediator);
    CommandResponse<TResponse> Execute(TCommand command, IUndoableMediator mediator);
}

public interface ICommandHandler
{
    void Undo(ICommand command, IUndoableMediator mediator);
    CommandResponse Execute(ICommand command, IUndoableMediator mediator);
}