using UndoableMediator.Requests;

namespace UndoableMediator.Commands;

public interface ICommandResponse
{
    RequestStatus Status { get; }
}

public interface ICommandResponse<TResponse> : ICommandResponse
{
    TResponse? Response { get; }
}