using UndoableMediator.Commands;
using UndoableMediator.Mediators;
using UndoableMediator.Requests;

namespace UndoableMediator.TestModels;

public class CancelableCommandHandler : CommandHandlerBase<CancelableCommand, bool>
{
    public CancelableCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override async Task<ICommandResponse<bool>> ExecuteAsync(CancelableCommand command)
    {
        var query = new CancelableQuery(command.ShouldBeCanceled);
        var result = await _mediator.QueryAsync(query);

        if (result == null)
        {
            return CommandResponse.Failed<bool>();
        }

        return result.Status == RequestStatus.Canceled ? CommandResponse.Canceled<bool>() : CommandResponse.Success(true);
    }
}