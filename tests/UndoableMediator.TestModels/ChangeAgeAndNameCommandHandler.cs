using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeAgeAndNameCommandHandler : CommandHandlerBase<ChangeAgeAndNameCommand>
{
    public ChangeAgeAndNameCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override CommandResponse Execute(ChangeAgeAndNameCommand command)
    {
        var changeAgeCmd = new ChangeAgeCommand(command.Age);
        var changeNameCmd = new ChangeNameCommand(command.Name);

        _mediator.Execute(changeAgeCmd);
        command.AddToSubCommands(changeAgeCmd);

        _mediator.Execute(changeNameCmd);
        command.AddToSubCommands(changeNameCmd);

        return CommandResponse.Success();
    }
}
