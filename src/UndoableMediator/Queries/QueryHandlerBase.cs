using UndoableMediator.Requests;

namespace UndoableMediator.Queries;

/// <summary>
///     Base class to inherit from when implementing the handler for a query
/// </summary>
/// <typeparam name="TQuery"> The query type handled by this handler </typeparam>
/// <typeparam name="TResponse"> Must correspond to the type param of the Query </typeparam>
public abstract class QueryHandlerBase<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : class, IQuery<TResponse>
{
    /// <inheritdoc />
    public abstract Task<IQueryResponse<TResponse>> ExecuteAsync(TQuery query);

    /// <inheritdoc />
    public async Task<IQueryResponse> ExecuteAsync(IQuery query)
    {
        var castedQuery = RequestCastHelper.CastOrThrow<TQuery>(query, "execute");
        return await ExecuteAsync(castedQuery);
    }
}