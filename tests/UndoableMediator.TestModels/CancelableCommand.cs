using UndoableMediator.Commands;

namespace UndoableMediator.TestModels;

public class CancelableCommand : CommandBase
{
    public CancelableCommand()
    {
    }

    public bool ShouldBeCanceled { get; }

    public CancelableCommand(bool shouldBeCanceled)
    {
        ShouldBeCanceled = shouldBeCanceled;
    }
}