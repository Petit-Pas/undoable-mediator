using UndoableMediator.Requests;

namespace UndoableMediator.Queries;

/// <summary>
///     Class that holds a request status and an response for a query
/// </summary>
/// <typeparam name="TResponse"> The answer type for the query </typeparam>
public class QueryResponse<TResponse> : ResponseBase<TResponse>, IQueryResponse<TResponse>
{
    internal QueryResponse(TResponse response, RequestStatus status) : base(response, status)
    {
    }

    /// <summary>
    ///     Creates a QueryResponse with a status of Canceled and the given content
    /// </summary>
    /// <typeparam name="TResponse"> Type expected as answer from the query </typeparam>
    /// <param name="response"> Answer of the query </param>
    /// <returns> The built QueryResponse </returns>
    public static IQueryResponse<TResponse> Canceled(TResponse response)
    {
		return new QueryResponse<TResponse>(response, RequestStatus.Canceled);
    }

    /// <summary>
    ///     Creates a QueryResponse with a status of Success and the given content
    /// </summary>
    /// <typeparam name="TResponse"> Type expected as answer from the query </typeparam>
    /// <param name="response"> Answer of the query </param>
    /// <returns> The built QueryResponse </returns>
    public static IQueryResponse<TResponse> Success(TResponse response)
    {
        return new QueryResponse<TResponse>(response, RequestStatus.Success);
    }

    /// <summary>
    ///     Creates a QueryResponse with a status of Failed and the given content
    /// </summary>
    /// <typeparam name="TResponse"> Type expected as answer from the query </typeparam>
    /// <param name="response"> Answer of the query </param>
    /// <returns> The built QueryResponse </returns>
    public static IQueryResponse<TResponse> Failed(TResponse response)
    {
        return new QueryResponse<TResponse>(response, RequestStatus.Failed);
    }

}
