namespace UndoableMediator.Queries;

public interface IQueryHandler<TQuery, TResponse> : IQueryHandler
    where TQuery : IQuery<TResponse>
{
    IQueryResponse<TResponse> Execute(TQuery query);
}

public interface IQueryHandler
{
    IQueryResponse Execute(IQuery query);
}
