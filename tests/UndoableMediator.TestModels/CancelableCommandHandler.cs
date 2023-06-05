using UndoableMediator.Commands;
using UndoableMediator.Mediators;
using UndoableMediator.Requests;

namespace UndoableMediator.TestModels;

public class CancelableCommandHandler : CommandHandlerBase<CancelableCommand, bool>
{
    public CancelableCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override CommandResponse<bool> Execute(CancelableCommand command)
    {
        var query = new CancelableQuery(command.ShouldBeCanceled);
        var result = _mediator.Execute(query);

        if (result == null)
        {
            return CommandResponse.Failed<bool>();
        }

        return result.Status == RequestStatus.Canceled ? CommandResponse.Canceled<bool>() : CommandResponse.Success(true);
    }
}