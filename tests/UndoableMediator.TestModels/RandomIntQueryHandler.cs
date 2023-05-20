using UndoableMediator.Queries;

namespace UndoableMediator.TestModels;

public class RandomIntQueryHandler : QueryHandlerBase<RandomIntQuery, int>
{
    public override IQueryResponse<int> Execute(RandomIntQuery query)
    {
        return QueryResponse<int>.Success(new Random().Next() % query.Range);
    }
}
