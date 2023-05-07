using System.Diagnostics;

namespace UndoableMediator.Queries;

public class QueryResponse<T> : IQueryResponse<T>
{
    private QueryResponse(T response, bool canceled)
    {
        Response = response;
        WasCanceled = canceled;
    }

    public static QueryResponse<T> Canceled(T response)
    {
		return new QueryResponse<T>(response, true);
    }

    public static QueryResponse<T> Success(T response)
    {
        return new QueryResponse<T>(response, false);
    }

    public T? Response { get; }
	public bool WasCanceled { get; }
}
