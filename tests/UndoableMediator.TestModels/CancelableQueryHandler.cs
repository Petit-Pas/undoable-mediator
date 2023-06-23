using UndoableMediator.Queries;

namespace UndoableMediator.TestModels;

public class CancelableQueryHandler : QueryHandlerBase<CancelableQuery, bool>
{
    public override Task<IQueryResponse<bool>> Execute(CancelableQuery query)
    {
        return Task.FromResult(query.ShouldBeCanceled ? QueryResponse<bool>.Canceled(false) : QueryResponse<bool>.Success(true));
    }
}