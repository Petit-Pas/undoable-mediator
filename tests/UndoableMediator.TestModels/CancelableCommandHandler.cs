using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class CancelableCommandHandler : CommandHandlerBase<CancelableCommand>
{
    public override CommandResponse Execute(CancelableCommand command, IUndoableMediator mediator)
    {
        var query = new CancelableQuery(command.ShouldBeCanceled);
        var result = query.ExecuteBy(mediator);

        if (result == null)
        {
            return CommandResponse.Failed;
        }

        return result.WasCanceled ? CommandResponse.Canceled : CommandResponse.Success;
    }
}