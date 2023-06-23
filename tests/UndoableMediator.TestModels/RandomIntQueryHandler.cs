using UndoableMediator.Queries;

namespace UndoableMediator.TestModels;

public class RandomIntQueryHandler : QueryHandlerBase<RandomIntQuery, int>
{
    public override Task<IQueryResponse<int>> Execute(RandomIntQuery query)
    {
        return Task.FromResult(QueryResponse<int>.Success(new Random().Next() % query.Range));
    }
}
