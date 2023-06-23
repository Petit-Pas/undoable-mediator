using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeAgeAndNameCommandHandler : CommandHandlerBase<ChangeAgeAndNameCommand>
{
    public ChangeAgeAndNameCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override async Task<ICommandResponse<NoResponse>> Execute(ChangeAgeAndNameCommand command)
    {
        var changeAgeCmd = new ChangeAgeCommand(command.Age);
        var changeNameCmd = new ChangeNameCommand(command.Name);

        await _mediator.Execute(changeAgeCmd);
        command.AddToSubCommands(changeAgeCmd);

        await _mediator.Execute(changeNameCmd);
        command.AddToSubCommands(changeNameCmd);

        return CommandResponse.Success();
    }
}
