using UndoableMediator.Queries;

namespace UndoableMediator.TestModels;

public class CancelableQueryHandler : QueryHandlerBase<CancelableQuery, bool>
{
    public override IQueryResponse<bool> Execute(CancelableQuery query)
    {
        return query.ShouldBeCanceled ? QueryResponse<bool>.Canceled(false) : QueryResponse<bool>.Success(false);
    }
}