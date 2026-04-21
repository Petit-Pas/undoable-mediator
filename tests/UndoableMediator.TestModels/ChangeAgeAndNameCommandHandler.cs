using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeAgeAndNameCommandHandler : CommandHandlerBase<ChangeAgeAndNameCommand>
{
    public ChangeAgeAndNameCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override async Task<ICommandResponse<NoResponse>> ExecuteAsync(ChangeAgeAndNameCommand command)
    {
        await _mediator.SendAsSubCommandAsync(new ChangeAgeCommand(command.Age), command);
        await _mediator.SendAsSubCommandAsync(new ChangeNameCommand(command.Name), command);
        return CommandResponse.Success();
    }
}
