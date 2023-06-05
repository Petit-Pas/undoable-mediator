using UndoableMediator.Requests;

namespace UndoableMediator.Queries;

/// <summary>
///     Class that holds a request status and an response for a query
/// </summary>
/// <typeparam name="TResponse"> The answer type for the query </typeparam>
public class QueryResponse<TResponse> : IQueryResponse<TResponse>
{
    internal QueryResponse(TResponse response, RequestStatus status)
    {
        Response = response;
        Status = status;
    }

    /// <summary>
    ///     Creates a QueryResponse with a status of Canceled and the given content
    /// </summary>
    /// <typeparam name="TResponse"> Type expected as answer from the query </typeparam>
    /// <param name="response"> Answer of the query </param>
    /// <returns> The build QueryResponse </returns>
    public static QueryResponse<TResponse> Canceled(TResponse response)
    {
		return new QueryResponse<TResponse>(response, RequestStatus.Canceled);
    }

    /// <summary>
    ///     Creates a QueryResponse with a status of Canceled and the given content
    /// </summary>
    /// <typeparam name="TResponse"> Type expected as answer from the query </typeparam>
    /// <param name="response"> Answer of the query </param>
    /// <returns> The build QueryResponse </returns>
    public static QueryResponse<TResponse> Success(TResponse response)
    {
        return new QueryResponse<TResponse>(response, RequestStatus.Success);
    }

    /// <summary>
    ///     Creates a QueryResponse with a status of Canceled and the given content
    /// </summary>
    /// <typeparam name="TResponse"> Type expected as answer from the query </typeparam>
    /// <param name="response"> Answer of the query </param>
    /// <returns> The build QueryResponse </returns>
    public static QueryResponse<TResponse> Failed(TResponse response)
    {
        return new QueryResponse<TResponse>(response, RequestStatus.Failed);
    }

    // <inheritdoc />
    public TResponse? Response { get; }
    // <inheritdoc />
    public RequestStatus Status { get; }
}
