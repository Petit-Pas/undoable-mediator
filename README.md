# UndoableMediator

A .NET library implementing the Mediator pattern with built-in **undo/redo** support.  
Commands can form trees of sub-commands, and undoing a parent automatically cascades to all its children.

## Quick Start

### 1. Register the Mediator

```csharp
builder.Services.ConfigureMediator(options =>
{
    options.AssembliesToScan = new[] { typeof(MyCommand).Assembly };
});
```

The mediator is registered as a **singleton**. Handlers are discovered via assembly scanning and registered as **transient**.

### 2. Define a Command & Handler

```csharp
// Command — inherits CommandBase (no return value) or CommandBase<TResponse> (with return value)
public class ChangeAgeCommand : CommandBase
{
    public int NewAge { get; }
    public int OldAge { get; set; } // saved during execution for undo
    public ChangeAgeCommand(int newAge) => NewAge = newAge;
}

// Handler — inherits CommandHandlerBase<TCommand> or CommandHandlerBase<TCommand, TResponse>
public class ChangeAgeCommandHandler : CommandHandlerBase<ChangeAgeCommand>
{
    public ChangeAgeCommandHandler(IUndoableMediator mediator) : base(mediator) { }

    public override Task<ICommandResponse<NoResponse>> ExecuteAsync(ChangeAgeCommand command)
    {
        command.OldAge = Person.Age;
        Person.Age = command.NewAge;
        return Task.FromResult(CommandResponse.Success());
    }

    public override async Task UndoAsync(ChangeAgeCommand command)
    {
        await base.UndoAsync(command); // propagate undo to sub-commands first
        Person.Age = command.OldAge;
    }

    public override async Task RedoAsync(ChangeAgeCommand command)
    {
        Person.Age = command.NewAge;
        await base.RedoAsync(command); // propagate redo to sub-commands
    }
}
```

### 3. Define a Query & Handler

Queries are read-only and cannot be undone.

```csharp
// Query — inherits QueryBase<TResponse>
public class RandomIntQuery : QueryBase<int> { }

// Handler — inherits QueryHandlerBase<TQuery, TResponse>
public class RandomIntQueryHandler : QueryHandlerBase<RandomIntQuery, int>
{
    public override Task<IQueryResponse<int>> ExecuteAsync(RandomIntQuery query)
        => Task.FromResult(QueryResponse<int>.Success(Random.Shared.Next(1000)));
}
```

### 4. Use the Mediator

```csharp
// Execute — automatically added to undo history on success
await _mediator.SendAsync(new ChangeAgeCommand(25));

// Query (read-only, never tracked)
var result = await _mediator.QueryAsync(new RandomIntQuery());

// Undo / Redo
await _mediator.UndoLastCommandAsync();
await _mediator.RedoLastUndoneCommandAsync();
```

## Sub-Commands

A handler can dispatch **sub-commands** via `SendAsSubCommandAsync`. When the parent is undone, all sub-commands are automatically undone in reverse order.

```csharp
public override async Task<ICommandResponse<NoResponse>> ExecuteAsync(CompositeCommand command)
{
    await _mediator.SendAsSubCommandAsync(new ChangeAgeCommand(command.Age), parentCommand: command);
    await _mediator.SendAsSubCommandAsync(new ChangeNameCommand(command.Name), parentCommand: command);
    return CommandResponse.Success();
}
// No need to override UndoAsync/RedoAsync — the base class handles sub-command propagation.
```

## Undo/Redo Behavior

| Rule | Details |
|------|---------|
| Auto-tracked | Commands are automatically added to history when `SendAsync` returns `RequestStatus.Success`. |
| Undo | Pops the last command from history, calls `UndoAsync`, moves it to the redo stack. |
| Redo | Pops from the redo stack, calls `RedoAsync`, pushes back to history. |
| Redo cleared on new command | Adding a new command to history clears the entire redo stack. |
| Bounded history | When `CommandHistoryMaxSize` is reached, the oldest command is discarded. |

## Override Contract

When overriding `UndoAsync` or `RedoAsync`:

- **Call `base.UndoAsync(command)` / `base.RedoAsync(command)`** to preserve automatic sub-command propagation, **or** handle sub-commands manually.
- For redo, you may alternatively call `ClearSubCommands(command)` then `ExecuteAsync(command)` to re-execute from scratch.

## Base Classes Reference

| Class | Purpose |
|-------|---------|
| `CommandBase` | Command with no return value |
| `CommandBase<TResponse>` | Command that returns `TResponse` |
| `CommandHandlerBase<TCommand>` | Handler for `CommandBase` commands |
| `CommandHandlerBase<TCommand, TResponse>` | Handler for `CommandBase<TResponse>` commands |
| `QueryBase<TResponse>` | Read-only query returning `TResponse` |
| `QueryHandlerBase<TQuery, TResponse>` | Handler for queries |

## Response Factories

```csharp
// Commands
CommandResponse.Success()              // no content
CommandResponse.Success<int>(42)       // with content
CommandResponse.Failed()  / .Failed<T>()
CommandResponse.Canceled()/ .Canceled<T>()

// Queries
QueryResponse<int>.Success(42)
QueryResponse<int>.Failed(0)
QueryResponse<int>.Canceled(0)
```

## Configuration

```csharp
builder.Services.ConfigureMediator(options =>
{
    options.CommandHistoryMaxSize = 64;      // default: 64
    options.RedoHistoryMaxSize = 32;         // default: 32
    options.AssembliesToScan = new[] { ... };
    options.ShouldScanAutomatically = false; // default: false
});
```

## Architecture Notes

- **Singleton mediator** — designed for desktop / single-user scenarios with a shared undo history.
- **Not thread-safe** — concurrent calls may corrupt history. This is intentional for the single-user use case.
- Handlers are resolved from `IServiceProvider` on each call (transient or scoped).

---

## Appendix A — Full API Reference

### IUndoableMediator

| Member | Signature |
|--------|-----------|
| `SendAsync` | `Task<ICommandResponse<T>> SendAsync<T>(ICommand<T> command)` |
| `SendAsSubCommandAsync` | `Task<ICommandResponse<T>> SendAsSubCommandAsync<T>(ICommand<T> subCommand, ICommand parentCommand)` |
| `QueryAsync` | `Task<IQueryResponse<T>> QueryAsync<T>(IQuery<T> query)` |
| `UndoLastCommandAsync` | `Task<bool> UndoLastCommandAsync()` |
| `RedoLastUndoneCommandAsync` | `Task<bool> RedoLastUndoneCommandAsync()` |
| `HistoryLength` | `int` — current undo history size |
| `RedoHistoryLength` | `int` — current redo history size |

### Handler Methods

| Method | Required | Notes |
|--------|----------|-------|
| `ExecuteAsync(TCommand)` | Yes | Implement command/query logic |
| `UndoAsync(TCommand)` | No | Call `base.UndoAsync` to propagate to sub-commands |
| `RedoAsync(TCommand)` | No | Call `base.RedoAsync` or `ClearSubCommands` + `ExecuteAsync` |

## Appendix B — Advanced: Command with Sub-Commands and Direct Mutations

A handler that both mutates state directly **and** dispatches sub-commands:

```csharp
public class UpdateAllCommandHandler : CommandHandlerBase<UpdateAllCommand>
{
    public UpdateAllCommandHandler(IUndoableMediator mediator) : base(mediator) { }

    public override async Task<ICommandResponse<NoResponse>> ExecuteAsync(UpdateAllCommand command)
    {
        // 1. Direct mutation — save old value for undo
        command.OldScore = Model.Score;
        Model.Score = command.NewScore;

        // 2. Sub-commands — automatically tracked for undo/redo
        await _mediator.SendAsSubCommandAsync(new ChangeAgeCommand(command.Age), parentCommand: command);
        await _mediator.SendAsSubCommandAsync(new ChangeNameCommand(command.Name), parentCommand: command);

        return CommandResponse.Success();
    }

    public override async Task UndoAsync(UpdateAllCommand command)
    {
        await base.UndoAsync(command);     // undo sub-commands (reverse order)
        Model.Score = command.OldScore;    // undo direct mutation
    }

    public override async Task RedoAsync(UpdateAllCommand command)
    {
        Model.Score = command.NewScore;    // redo direct mutation
        await base.RedoAsync(command);     // redo sub-commands (original order)
    }
}
```

## Appendix C — Advanced: Re-Execute Strategy for Redo

Instead of replaying recorded sub-commands, clear them and re-execute from scratch:

```csharp
public override async Task RedoAsync(MyCommand command)
{
    ClearSubCommands(command);      // discard stale sub-commands
    await ExecuteAsync(command);    // re-runs full logic, re-dispatches sub-commands
}
```

## License

See [LICENSE](LICENSE) for details.
