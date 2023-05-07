using UndoableMediator.Commands;

namespace UndoableMediator.TestModels;

public class CancelableCommand : CommandBase
{
    public bool ShouldBeCanceled { get; }

    public CancelableCommand(bool shouldBeCanceled)
    {
        ShouldBeCanceled = shouldBeCanceled;
    }
}