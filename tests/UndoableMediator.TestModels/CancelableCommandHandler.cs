using UndoableMediator.Commands;
using UndoableMediator.Mediators;
using UndoableMediator.Requests;

namespace UndoableMediator.TestModels;

public class CancelableCommandHandler : CommandHandlerBase<CancelableCommand>
{
    public override CommandResponse Execute(CancelableCommand command, IUndoableMediator mediator)
    {
        var query = new CancelableQuery(command.ShouldBeCanceled);
        var result = mediator.Execute(query);

        if (result == null)
        {
            return CommandResponse.Failed();
        }

        return result.Status == RequestStatus.Canceled ? CommandResponse.Canceled() : CommandResponse.Success();
    }
}