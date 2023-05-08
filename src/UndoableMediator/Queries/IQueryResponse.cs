using UndoableMediator.Requests;

namespace UndoableMediator.Queries;

public interface IQueryResponse<T> : IQueryResponse
{
    T? Response { get; }
}

public interface IQueryResponse
{
    RequestStatus Status { get; }
}
