using UndoableMediator.Mediators;

namespace UndoableMediator.Commands;

public interface ICommandHandler<TCommand> : ICommandHandler
    where TCommand : class, ICommand
{
    void Undo(TCommand command);
    ICommandResponse Execute(TCommand command);
}

public interface ICommandHandler<TCommand, TResponse> : ICommandHandler
    where TCommand : class, ICommand<TResponse>
{
    void Undo(TCommand command);
    ICommandResponse<TResponse> Execute(TCommand command);
}

public interface ICommandHandler
{
    void Undo(ICommand command);
    ICommandResponse Execute(ICommand command);
}