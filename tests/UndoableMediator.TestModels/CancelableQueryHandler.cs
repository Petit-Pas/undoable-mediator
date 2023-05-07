using UndoableMediator.Queries;

namespace UndoableMediator.TestModels;

public class CancelableQueryHandler : IQueryHandler<CancelableQuery, bool>
{
    public IQueryResponse<bool> Execute(CancelableQuery query)
    {
        return query.ShouldBeCanceled ? QueryResponse<bool>.Canceled(false) : QueryResponse<bool>.Success(false);
    }
}