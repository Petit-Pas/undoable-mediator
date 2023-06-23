﻿using UndoableMediator.Commands;
using UndoableMediator.Mediators;

namespace UndoableMediator.TestModels;

public class ChangeNameCommandHandler : CommandHandlerBase<ChangeNameCommand>
{
    public ChangeNameCommandHandler(IUndoableMediator mediator) : base(mediator)
    {
    }

    public override Task<ICommandResponse<NoResponse>> Execute(ChangeNameCommand command)
    {
        command.OldName = AffectedObject.Name;
        AffectedObject.Name = command.NewName;

        return Task.FromResult(CommandResponse.Success());
    }
    
    public override void Undo(ChangeNameCommand command)
    {
        if (command.OldName != null)
        {
            AffectedObject.Name = command.OldName;
        }
    }

    public override void Redo(ChangeNameCommand command)
    {
        Execute(command);
    }
}
