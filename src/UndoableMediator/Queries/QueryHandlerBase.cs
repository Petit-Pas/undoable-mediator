namespace UndoableMediator.Queries;

/// <summary>
///     Base class to inherit from when implementing the handler for a query
/// </summary>
/// <typeparam name="TQuery"> The query type handled by this handler </typeparam>
/// <typeparam name="TResponse"> Must correspond to the type param of the Query </typeparam>
public abstract class QueryHandlerBase<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : class, IQuery<TResponse>
{
    // <inheritdoc />
    public abstract IQueryResponse<TResponse> Execute(TQuery query);

    // <inheritdoc />
    public IQueryResponse Execute(IQuery query)
    {
        if (query is TQuery castedCommand)
        {
            return Execute(castedCommand);
        }
        throw new InvalidOperationException($"Cannot execute query of type {query.GetType().FullName} because it is not of type {typeof(TQuery).FullName}");
    }
}