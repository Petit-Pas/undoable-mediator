using UndoableMediator.Requests;

namespace UndoableMediator.Queries;

public class QueryResponse<T> : IQueryResponse<T>
{
    internal QueryResponse(T response, RequestStatus status)
    {
        Response = response;
        Status = status;
    }

    public static QueryResponse<T> Canceled(T response)
    {
		return new QueryResponse<T>(response, RequestStatus.Canceled);
    }

    public static QueryResponse<T> Success(T response)
    {
        return new QueryResponse<T>(response, RequestStatus.Success);
    }

    public static QueryResponse<T> Failed(T response)
    {
        return new QueryResponse<T>(response, RequestStatus.Failed);
    }

    public T? Response { get; }
	public RequestStatus Status { get; }
}
