namespace UndoableMediator.Queries;

public abstract class QueryHandlerBase<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : class, IQuery<TResponse>
{
    public abstract IQueryResponse<TResponse> Execute(TQuery query);

    public IQueryResponse Execute(IQuery query)
    {
        return this.Execute(query as TQuery);
    }
}