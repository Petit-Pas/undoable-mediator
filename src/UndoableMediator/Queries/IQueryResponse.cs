using UndoableMediator.Requests;

namespace UndoableMediator.Queries;

/// <summary>
///     Base generic interface for query responses, holds the response as well as the status
/// </summary>
/// <typeparam name="TResponse"> The type of the response </typeparam>
public interface IQueryResponse<TResponse> : IQueryResponse
{
    /// <summary>
    ///     The potential response of the query
    /// </summary>
    TResponse? Response { get; }
}

/// <summary>
///     Base interface for query responses, only holds status
/// </summary>
public interface IQueryResponse
{
    /// <summary>
    ///     The status of the query
    /// </summary>
    RequestStatus Status { get; }
}
