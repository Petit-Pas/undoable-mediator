namespace UndoableMediator.Queries;

public abstract class QueryHandlerBase<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : class, IQuery<TResponse>
{
    public abstract IQueryResponse<TResponse> Execute(TQuery query);

    public IQueryResponse Execute(IQuery query)
    {
        if (query is TQuery castedCommand)
        {
            return Execute(castedCommand);
        }
        throw new InvalidOperationException($"Cannot execute query of type {query.GetType().FullName} because it is not of type {typeof(TQuery).FullName}");
    }
}