namespace UndoableMediator.Queries;

/// <summary>
///     Base generic interface for queries
/// </summary>
/// <typeparam name="TReturn"> Type of the answer expected. </typeparam>
public interface IQuery<TReturn> : IQuery
{
}

/// <summary>
///     Base interface for queries
/// </summary>
public interface IQuery
{
}
