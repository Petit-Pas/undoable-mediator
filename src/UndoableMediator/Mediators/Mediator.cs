using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.Logging;
using UndoableMediator.Commands;
using UndoableMediator.DependencyInjection;
using UndoableMediator.Queries;
using UndoableMediator.Requests;

namespace UndoableMediator.Mediators;

public class Mediator : IUndoableMediator, ISubCommandDispatcher
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    internal int CommandHistoryMaxSize { get; }
    internal int CommandRedoHistoryMaxSize { get; }

    internal readonly LinkedList<ICommand> CommandHistory;
    internal readonly LinkedList<ICommand> RedoHistory;

    public int HistoryLength => CommandHistory.Count;
    public int RedoHistoryLength => RedoHistory.Count;

    public Mediator(ILogger<IUndoableMediator> logger, IServiceProvider serviceProvider, UndoableMediatorOptions options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        CommandHistoryMaxSize = options.CommandHistoryMaxSize;
        CommandRedoHistoryMaxSize = options.RedoHistoryMaxSize;

        CommandHistory = new LinkedList<ICommand>();
        RedoHistory = new LinkedList<ICommand>();
    }

    // ──────────────────────────────────────────────
    //  Public API — Execute & Query
    // ──────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<ICommandResponse<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var handler = GetCommandHandlerFor(command);
        var response = await handler.ExecuteAsync(command);

        if (response.Status == RequestStatus.Success)
        {
            AddCommandToHistory(command);
        }

        return (ICommandResponse<TResponse>)response;
    }

    /// <inheritdoc />
    public async Task<ICommandResponse<TSubResponse>> SendAsSubCommandAsync<TSubResponse>(ICommand<TSubResponse> subCommand, ICommand parentCommand)
    {
        ArgumentNullException.ThrowIfNull(subCommand);
        ArgumentNullException.ThrowIfNull(parentCommand);

        if (parentCommand is not ISubCommandHost host)
        {
            throw new ArgumentException($"Parent command of type {parentCommand.GetType().FullName} does not implement {nameof(ISubCommandHost)}. Only commands deriving from {nameof(CommandBase)} support sub-commands.", nameof(parentCommand));
        }

        var result = await SendAsync(subCommand);
        host.AddSubCommand(subCommand);
        return result;
    }

    /// <inheritdoc />
    public async Task<IQueryResponse<TResponse>> QueryAsync<TResponse>(IQuery<TResponse> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        var handler = GetQueryHandlerFor(query);
        return (IQueryResponse<TResponse>)await handler.ExecuteAsync(query);
    }

    // ──────────────────────────────────────────────
    //  Public API — Undo & Redo
    // ──────────────────────────────────────────────

    /// <inheritdoc />
    public async Task<bool> UndoLastCommandAsync()
    {
        if (CommandHistory.Count == 0)
        {
            return false;
        }

        var lastCommand = CommandHistory.Last!.Value;

        await DispatchUndoAsync(lastCommand);

        CommandHistory.RemoveLast();
        RedoHistory.AddLast(lastCommand);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RedoLastUndoneCommandAsync()
    {
        if (RedoHistory.Count == 0) 
        {
            return false;
        }

        var lastCommandUndone = RedoHistory.Last!.Value;

        await DispatchRedoAsync(lastCommandUndone);

        MoveLastCommandFromRedoHistoryToHistory();
        return true;
    }

    // ──────────────────────────────────────────────
    //  Internal dispatch — used by the public API above
    //  and by CommandHandlerBase for sub-command propagation
    // ──────────────────────────────────────────────

    /// <summary>
    ///     Resolves the handler for <paramref name="command"/> and calls its UndoAsync.
    /// </summary>
    private async Task DispatchUndoAsync(ICommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var handler = GetCommandHandlerForUntyped(command);
        await handler.UndoAsync(command);
    }

    /// <summary>
    ///     Resolves the handler for <paramref name="command"/> and calls its RedoAsync.
    /// </summary>
    private async Task DispatchRedoAsync(ICommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var handler = GetCommandHandlerForUntyped(command);
        await handler.RedoAsync(command);
    }

    // Explicit interface — delegates to the private methods above.
    async Task ISubCommandDispatcher.UndoAsync(ICommand command) => await DispatchUndoAsync(command);
    async Task ISubCommandDispatcher.RedoAsync(ICommand command) => await DispatchRedoAsync(command);

    // ──────────────────────────────────────────────
    //  History management
    // ──────────────────────────────────────────────

    private void AddCommandToHistory(ICommand command)
    {
        if (CommandHistory.Count == CommandHistoryMaxSize)
        {
            CommandHistory.RemoveFirst();
        }

        CommandHistory.AddLast(command);
        RedoHistory.Clear();
    }

    private void MoveLastCommandFromRedoHistoryToHistory()
    {
        if (RedoHistory.Count == 0)
        {
            return;
        }

        var command = RedoHistory.Last!.Value;
        RedoHistory.RemoveLast();

        if (CommandHistory.Count == CommandHistoryMaxSize)
        {
            CommandHistory.RemoveFirst();
        }

        CommandHistory.AddLast(command);
    }

    // ──────────────────────────────────────────────
    //  Handler resolution (with caching)
    // ──────────────────────────────────────────────

    private readonly ConcurrentDictionary<(Type OpenHandler, Type Request, Type Response), Type> _handlerTypeCache = new();
    private readonly ConcurrentDictionary<Type, MethodInfo> _untypedMethodCache = new();

    private THandler ResolveHandler<THandler>(Type openHandlerType, Type requestType, Type responseType, string requestKind) where THandler : class
    {
        var closedHandlerType = _handlerTypeCache.GetOrAdd(
            (openHandlerType, requestType, responseType),
            key => key.OpenHandler.MakeGenericType(key.Request, key.Response));

        return _serviceProvider.GetService(closedHandlerType) as THandler
            ?? throw new NotImplementedException($"Missing {requestKind} handler for {requestType.FullName}.");
    }

    private ICommandHandler GetCommandHandlerFor<TResponse>(ICommand<TResponse> command)
    {
        return ResolveHandler<ICommandHandler>(typeof(ICommandHandler<,>), command.GetType(), typeof(TResponse), "command");
    }

    private IQueryHandler GetQueryHandlerFor<TResponse>(IQuery<TResponse> query)
    {
        return ResolveHandler<IQueryHandler>(typeof(IQueryHandler<,>), query.GetType(), typeof(TResponse), "query");
    }

    /// <summary>
    ///     Resolves a command handler when only the non-generic <see cref="ICommand"/> is available
    ///     (i.e. during undo/redo where the TResponse is not known at compile time).
    /// </summary>
    private ICommandHandler GetCommandHandlerForUntyped(ICommand command)
    {
        var responseType = ReflectionHelper.GetGenericInterfaceArgument(command.GetType(), typeof(ICommand<>))
            ?? throw new NotImplementedException($"Could not find a handler for command of type {command.GetType().FullName}");

        var method = _untypedMethodCache.GetOrAdd(
            responseType,
            rt => ReflectionHelper.GetClosedPrivateMethod<Mediator>(nameof(GetCommandHandlerFor), 1, rt));

        return ReflectionHelper.InvokeUnwrapped(method, this, command) as ICommandHandler
            ?? throw new NotImplementedException($"Missing command handler for {command.GetType().FullName}.");
    }
}
