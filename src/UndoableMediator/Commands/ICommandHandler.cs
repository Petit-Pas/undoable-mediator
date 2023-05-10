using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

public interface ICommandHandler<TCommand> : ICommandHandler
    where TCommand : class, ICommand
{
    void Undo(TCommand command, IUndoableMediator mediator);
    ICommandResponse Execute(TCommand command, IUndoableMediator mediator);
}

public interface ICommandHandler<TCommand, TResponse> : ICommandHandler
    where TCommand : class, ICommand<TResponse>
{
    void Undo(TCommand command, IUndoableMediator mediator);
    ICommandResponse<TResponse> Execute(TCommand command, IUndoableMediator mediator);
}

public interface ICommandHandler
{
    void GenericUndo(ICommand command, IUndoableMediator mediator);
    ICommandResponse Execute(ICommand command, IUndoableMediator mediator);
}