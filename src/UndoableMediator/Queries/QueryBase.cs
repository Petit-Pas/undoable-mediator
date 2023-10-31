namespace UndoableMediator.Queries;

/// <summary>
///     Can be used as base class for any query
/// </summary>
/// <typeparam name="TResponse"> The type expected as response to the query </typeparam>
public abstract class QueryBase<TResponse> : IQuery<TResponse>
{
}
