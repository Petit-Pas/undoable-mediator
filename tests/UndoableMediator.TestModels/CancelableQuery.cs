using UndoableMediator.Queries;

namespace UndoableMediator.TestModels;

public class CancelableQuery : QueryBase<bool>
{
    public CancelableQuery(bool shouldBeCanceled =  false)
    {
        ShouldBeCanceled = shouldBeCanceled;
    }

    public bool ShouldBeCanceled { get; }
}