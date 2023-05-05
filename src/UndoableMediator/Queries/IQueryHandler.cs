namespace UndoableMediator.Queries;

public interface IQueryHandler<TQuery, TResponse>
{
    public IQueryResponse<TResponse> Execute(TQuery query);
}
