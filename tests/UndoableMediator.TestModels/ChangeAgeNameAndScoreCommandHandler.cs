using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeAgeNameAndScoreCommandHandler : CommandHandlerBase<ChangeAgeNameAndScoreCommand>
{
    public ChangeAgeNameAndScoreCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override async Task<ICommandResponse<NoResponse>> ExecuteAsync(ChangeAgeNameAndScoreCommand command)
    {
        command.OldScore = AffectedObject.Score;
        AffectedObject.Score = command.Score;

        await _mediator.SendAsSubCommandAsync(new ChangeAgeCommand(command.Age), command);
        await _mediator.SendAsSubCommandAsync(new ChangeNameCommand(command.Name), command);

        return CommandResponse.Success();
    }

    public override async Task UndoAsync(ChangeAgeNameAndScoreCommand command)
    {
        // Undo sub-commands first (LIFO: Name undone, then Age undone)
        await base.UndoAsync(command);
        // Then undo the direct mutation performed by this handler
        AffectedObject.Score = command.OldScore;
    }

    public override async Task RedoAsync(ChangeAgeNameAndScoreCommand command)
    {
        // Redo the direct mutation first (mirrors execute order)
        AffectedObject.Score = command.Score;
        // Then redo sub-commands (FIFO: Age redone, then Name redone)
        await base.RedoAsync(command);
    }
}
