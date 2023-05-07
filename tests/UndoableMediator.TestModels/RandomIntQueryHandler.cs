using UndoableMediator.Queries;

namespace UndoableMediator.TestModels;

public class RandomIntQueryHandler : IQueryHandler<RandomIntQuery, int>
{
    public IQueryResponse<int> Execute(RandomIntQuery _)
    {
        return QueryResponse<int>.Success(new Random().Next(1000));
    }
}
