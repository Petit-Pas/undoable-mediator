# Migration Guide

## From v1 to v2

### Breaking changes at a glance

| Area | What changed |
|------|-------------|
| Method naming | All `Execute` → `ExecuteAsync`, `Undo` → `UndoAsync`, `Redo` → `RedoAsync` |
| Mediator API | `Execute` overloads → `SendAsync`, `QueryAsync`, `SendAsSubCommandAsync` |
| Undo/Redo API | `UndoLastCommand()` (sync) → `UndoLastCommandAsync()` (async) |
| History tracking | Was opt-in via delegate — now automatic on `RequestStatus.Success` |
| Sub-commands | Manual `AddToSubCommands` + `Execute` → single `SendAsSubCommandAsync` call |
| Sub-command internals | `SubCommands` property removed from public API |

---

### 1. Mediator call sites

```diff
- var result = await _mediator.Execute(command, _ => true);
+ var result = await _mediator.SendAsync(command);

- var result = await _mediator.Execute(query);
+ var result = await _mediator.QueryAsync(query);

- _mediator.UndoLastCommand();
+ await _mediator.UndoLastCommandAsync();

- await _mediator.RedoLastUndoneCommand();
+ await _mediator.RedoLastUndoneCommandAsync();
```

**History tracking is now automatic.** Any `SendAsync` call that returns `RequestStatus.Success` is added to the undo history. The `shouldAddCommandToHistory` delegate parameter and `AddAlways` helper have been removed entirely.

If you previously used `Execute(command)` without a delegate (no history tracking), note that successful commands **will now be tracked**. If you need fire-and-forget commands that are never undone, return `RequestStatus.Failed` or `RequestStatus.Canceled` — or simply don't call undo.

### 2. Handler method names

All handler methods were renamed to follow the async naming convention:

```diff
  public class MyCommandHandler : CommandHandlerBase<MyCommand>
  {
-     public override Task<ICommandResponse<NoResponse>> Execute(MyCommand command)
+     public override Task<ICommandResponse<NoResponse>> ExecuteAsync(MyCommand command)
      {
          // ...
      }

-     public override void Undo(MyCommand command)
+     public override async Task UndoAsync(MyCommand command)
      {
-         base.Undo(command);
+         await base.UndoAsync(command);
          // ...
      }

-     public override async Task Redo(MyCommand command)
+     public override async Task RedoAsync(MyCommand command)
      {
          // ...
-         await base.Redo(command);
+         await base.RedoAsync(command);
      }
  }
```

Query handlers follow the same pattern:

```diff
- public override Task<IQueryResponse<int>> Execute(MyQuery query)
+ public override Task<IQueryResponse<int>> ExecuteAsync(MyQuery query)
```

### 3. Sub-commands

The old two-step pattern (execute + manually register) is replaced by a single call:

```diff
  public override async Task<ICommandResponse<NoResponse>> ExecuteAsync(ParentCommand command)
  {
-     var sub = new ChildCommand(42);
-     await _mediator.Execute(sub);
-     command.AddToSubCommands(sub);
+     await _mediator.SendAsSubCommandAsync(new ChildCommand(42), parentCommand: command);
      return CommandResponse.Success();
  }
```

The `SubCommands` property and `AddToSubCommands` method have been removed from the public `ICommand` interface. Sub-command management is now fully internal.

**New helper — `ClearSubCommands`:** Use in handlers that re-execute on redo:

```csharp
public override async Task RedoAsync(MyCommand command)
{
    ClearSubCommands(command);   // discard stale sub-commands
    await ExecuteAsync(command); // re-runs full logic
}
```

### 4. Undo/Redo is now fully async

`UndoLastCommand()` was synchronous (`bool`). It is now `Task<bool> UndoLastCommandAsync()`.
`Undo(TCommand)` in handlers was `void`. It is now `Task UndoAsync(TCommand)`.

Ensure all call sites are `await`ed.

### 5. Redo sub-command order fix

Redo now iterates sub-commands in **original execution order** (FIFO) instead of reverse. This matches the expected semantic: when redoing a parent, children are re-applied in the same order they were originally executed.

### 6. `Undo` and `Redo` removed from `IUndoableMediator`

The single-command `Undo(ICommand)` and `Redo(ICommand)` methods have been removed from the public interface. They were internal implementation details that should not have been exposed. Undo/redo is now driven exclusively through `UndoLastCommandAsync()` and `RedoLastUndoneCommandAsync()`.

### 7. DI registration (non-breaking)

`ConfigureMediator` gained an optional `ILoggerFactory?` parameter. Existing code compiles without changes. Diagnostic output moved from `Console.WriteLine` to `ILogger`.

---

### Quick find-and-replace checklist

| Search | Replace |
|--------|---------|
| `.Execute(` (on mediator) | `.SendAsync(` or `.QueryAsync(` or `.SendAsSubCommandAsync(` |
| `Execute(` (in handler overrides) | `ExecuteAsync(` |
| `Undo(` (in handler overrides) | `UndoAsync(` |
| `Redo(` (in handler overrides) | `RedoAsync(` |
| `base.Undo(` | `await base.UndoAsync(` |
| `base.Redo(` | `await base.RedoAsync(` |
| `UndoLastCommand()` | `await UndoLastCommandAsync()` |
| `RedoLastUndoneCommand()` | `await RedoLastUndoneCommandAsync()` |
| `AddToSubCommands` | Remove — use `SendAsSubCommandAsync` instead |
| `addToHistory: true` | Remove — automatic on success |
| `_ => true` / `AddAlways` | Remove — automatic on success |
| `.SubCommands` | Remove — internal |
