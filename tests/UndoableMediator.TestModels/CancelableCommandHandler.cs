using UndoableMediator.Commands;
using UndoableMediator.Mediators;
using UndoableMediator.Requests;

namespace UndoableMediator.TestModels;

public class CancelableCommandHandler : CommandHandlerBase<CancelableCommand>
{
    public CancelableCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override CommandResponse Execute(CancelableCommand command)
    {
        var query = new CancelableQuery(command.ShouldBeCanceled);
        var result = _mediator.Execute<CancelableQuery, bool>(query);

        if (result == null)
        {
            return CommandResponse.Failed();
        }

        return result.Status == RequestStatus.Canceled ? CommandResponse.Canceled() : CommandResponse.Success();
    }
}