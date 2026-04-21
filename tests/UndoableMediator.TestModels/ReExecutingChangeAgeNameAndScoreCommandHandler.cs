using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ReExecutingChangeAgeNameAndScoreCommandHandler : CommandHandlerBase<ReExecutingChangeAgeNameAndScoreCommand>
{
    public ReExecutingChangeAgeNameAndScoreCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override async Task<ICommandResponse<NoResponse>> ExecuteAsync(ReExecutingChangeAgeNameAndScoreCommand command)
    {
        command.OldScore = AffectedObject.Score;
        AffectedObject.Score = command.Score;

        await _mediator.SendAsSubCommandAsync(new ChangeAgeCommand(command.Age), command);
        await _mediator.SendAsSubCommandAsync(new ChangeNameCommand(command.Name), command);

        return CommandResponse.Success();
    }

    public override async Task UndoAsync(ReExecutingChangeAgeNameAndScoreCommand command)
    {
        // Undo sub-commands first (LIFO: Name undone, then Age undone)
        await base.UndoAsync(command);
        // Then undo the direct mutation performed by this handler
        AffectedObject.Score = command.OldScore;
    }

    public override async Task RedoAsync(ReExecutingChangeAgeNameAndScoreCommand command)
    {
        // Discard stale sub-commands and re-execute from scratch.
        // This is useful when external state may have changed between undo and redo,
        // and the handler should re-query or re-compute rather than replay old values.
        ClearSubCommands(command);
        await ExecuteAsync(command);
    }
}
