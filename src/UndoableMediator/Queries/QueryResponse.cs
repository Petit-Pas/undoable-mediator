namespace UndoableMediator.Queries;

public class QueryResponse<T> : IQueryResponse<T>
{
	public QueryResponse(T response)
	{
		Response = response;
	}

    public T? Response { get; set; }
}
