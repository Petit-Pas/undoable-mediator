using UndoableMediator.Queries;

namespace UndoableMediator.TestModels;

public class RandomIntQueryHandler : IQueryHandler<RandomIntQuery, int>
{
    public IQueryResponse<int> Execute(RandomIntQuery _)
    {
        return new QueryResponse<int>(new Random().Next(1000));
    }
}
