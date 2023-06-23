namespace UndoableMediator.Queries;

/// <summary>
///     Base generic interface for query handlers
/// </summary>
/// <typeparam name="TQuery"> Type of the query to handle </typeparam>
/// <typeparam name="TResponse"> Type of the answer expected for the query, must match the generic parameter if the query. </typeparam>
public interface IQueryHandler<TQuery, TResponse> : IQueryHandler
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    ///     This is the actual Execute method that does the job, should be implemented by the handler implementation
    /// </summary>
    /// <param name="query"> The query to execute </param>
    /// <returns> The query response </returns>
    Task<IQueryResponse<TResponse>> Execute(TQuery query);
}

/// <summary>
///     Base interface for query handlers
/// </summary>
public interface IQueryHandler
{
    /// <summary>
    ///     Generic method, will be called first, then specialize into the other "Execute()"
    /// </summary>
    /// <param name="query"> The query to execute </param>
    /// <returns> The response of the query, can be casted into IQueryResponse<TResponse> </returns>
    /// <exception cref="InvalidOperationException"> Should never be thrown, unless being called wrongly by something else than the mediator </exception>
    Task<IQueryResponse> Execute(IQuery query);
}
