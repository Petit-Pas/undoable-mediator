using System.Reflection;
using Microsoft.Extensions.Logging;
using UndoableMediator.Commands;
using UndoableMediator.Queries;
using UndoableMediator.Requests;

namespace UndoableMediator.Mediators;

public class Mediator : IUndoableMediator
{
    private readonly ILogger _logger;

    // TODO decouple this from the mediator so that the GetCommandHandlerFor(command/query) can be extracted
    internal readonly Dictionary<Type, ICommandHandler?> _commandHandlers = new();
    internal readonly Dictionary<Type, IQueryHandler?> _queryHandlers = new();

    // Config
    internal static int CommandHistoryMaxSize { get; set; } = 64;
    internal static int CommandRedoHistoryMaxSize { get; set; } = 32;
    internal static Assembly[]? AdditionalAssemblies = Array.Empty<Assembly>();
    internal static bool ThrowsOnMissingHandler { get; set; }
    internal static bool ShouldScanAutomatically { get; set; }

    // TODO these could be replaced with a max sized stack
    internal readonly List<ICommand> _commandHistory;
    internal readonly List<ICommand> _redoHistory;
        
    public Mediator(ILogger<Mediator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (CommandHistoryMaxSize <= 0)
        {
            throw new InvalidOperationException($"Cannot build a Mediator with a CommandHistoryMaxSize of {CommandHistoryMaxSize}");
        }
        if (CommandRedoHistoryMaxSize <= 0)
        {
            throw new InvalidOperationException($"Cannot build a Mediator with a CommandRedoHistoryMaxSize of {CommandRedoHistoryMaxSize}");
        }
        _commandHistory = new List<ICommand>(CommandHistoryMaxSize);
        _redoHistory = new List<ICommand>(CommandRedoHistoryMaxSize);

        ScanAssemblies();
        SanityCheck();
    }

    internal void ScanAssemblies()
    {
        _logger.LogInformation("Mediator is scanning application for commands.");

        var assembliesToScan = ShouldScanAutomatically 
            ? AppDomain.CurrentDomain.GetAssemblies() 
            : Array.Empty<Assembly>();
        
        if (AdditionalAssemblies != null && AdditionalAssemblies.Length != 0)
        {
            assembliesToScan = assembliesToScan.Union(AdditionalAssemblies).ToArray();
        }

        foreach (var assembly in assembliesToScan)
        {
            _logger.LogDebug($"Mediator is scanning '{0}' assembly looking for commands", assembly.FullName);
            try
            {
                foreach (var implementationType in assembly.GetTypes())
                {
                    if (!implementationType.GetTypeInfo().IsAbstract)
                    {
                        foreach (var interfaceType in implementationType.GetInterfaces())
                        {
                            if (interfaceType == typeof(ICommand))
                            {
                                _logger.LogInformation(
                                    $"MediatorBase found the '{implementationType.FullName}' command.");
                                RegisterCommand(implementationType);
                            }
                            
                            else if (interfaceType == typeof(IQuery))
                            {
                                _logger.LogInformation(
                                    $"Mediator found the '{implementationType.FullName}' query.");
                                RegisterQuery(implementationType);
                            }
                            
                            else if (interfaceType.IsGenericType &&
                                     interfaceType.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                            {
                                var commandType = interfaceType.GetGenericArguments().Single();
                                _logger.LogInformation(
                                    $"Mediator found the '{implementationType.FullName}' command handler to handle {commandType.FullName}.");
                                RegisterCommandHandler(implementationType, commandType);
                            }
                            else if (interfaceType.IsGenericType &&
                                     interfaceType.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
                            {
                                var commandType = interfaceType.GetGenericArguments().First();
                                _logger.LogInformation(
                                    $"Mediator found the '{implementationType.FullName}' command handler to handle {commandType.FullName}.");
                                RegisterCommandHandler(implementationType, commandType);
                            }
                            
                            else if (interfaceType.IsGenericType &&
                                     interfaceType.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                            {
                                var queryType = interfaceType.GetGenericArguments().First();
                                _logger.LogInformation(
                                    $"Mediator found the '{implementationType.FullName}' query handler to handle {queryType.FullName}.");
                                RegisterQueryHandler(implementationType, queryType);
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                _logger.LogWarning($"BaseMediator could not load types from {assembly.FullName}");
            }
        }
    }

    internal void RegisterCommand(Type commandType)
    {
        if (!_commandHandlers.ContainsKey(commandType))
        {
            _commandHandlers.Add(commandType, null);
        }
        else
        {
            _logger.LogWarning(
                $"Mediator will not register command {commandType.FullName} because it was already known by Mediator.");
        }
    }

    internal void RegisterQuery(Type queryType)
    {
        if (!_queryHandlers.ContainsKey(queryType))
        {
            _queryHandlers.Add(queryType, null);
        }
        else
        {
            _logger.LogWarning(
                $"Mediator will not register query {queryType.FullName} because it was already known by Mediator.");
        }
    }

    internal void RegisterCommandHandler(Type commandHandlerType, Type commandType)
    {
        if (_commandHandlers.TryGetValue(commandType, out var registeredHandler))
        {
            if (registeredHandler != null)
            {
                _logger.LogWarning(
                    $"Mediator will override command handler {registeredHandler.GetType().FullName} with {commandHandlerType.FullName} " +
                    $"as the handler for {commandType.FullName} since a new value was found by Mediator");
            }
        }

        _commandHandlers[commandType] = Activator.CreateInstance(commandHandlerType) as ICommandHandler;
    }

    internal void RegisterQueryHandler(Type queryHandlerType, Type queryType)
    {
        if (_queryHandlers.TryGetValue(queryType, out var registeredHandler))
        {
            if (registeredHandler != null)
            {
                _logger.LogWarning(
                    $"Mediator will override query handler {registeredHandler.GetType().FullName} with {queryHandlerType.FullName} " +
                    $"as the handler for {queryType.FullName} since a new value was found by Mediator");
            }
        }

        _queryHandlers[queryType] = Activator.CreateInstance(queryHandlerType) as IQueryHandler;
    }

    internal void SanityCheck()
    {
        if ((HasSomeCommandsWithoutHandler() || HasSomeQueriesWithoutHandler()) && ThrowsOnMissingHandler)
        {
            _logger.LogError("There is at least 1 missing handler for a command or a query, and the flag to throw on that condition is set.");
            throw new NotImplementedException("There is at least 1 missing handler for a command or a query, and the flag to throw on that condition is set.");
        }
    }

    private bool HasSomeCommandsWithoutHandler()
    {
        var missingAtLeastOne = false;

        foreach (var (commandType, handlerType) in _commandHandlers)
        {
            if (handlerType == null)
            {
                _logger.LogWarning($"Did not find a corresponding handler for command {commandType.FullName}.");
                missingAtLeastOne = true;
            }
        }
        return missingAtLeastOne;
    }

    private bool HasSomeQueriesWithoutHandler()
    {
        var missingAtLeastOne = false;
        
        foreach (var (queryType, handlerType) in _queryHandlers)
        {
            if (handlerType == null)
            {
                _logger.LogWarning($"Did not find a corresponding handler for query {queryType.FullName}.");
                missingAtLeastOne = true;
            }
        }
        return missingAtLeastOne;
    }

    internal ICommandHandler GetCommandHandlerFor(Type commandType)
    {
        if (_commandHandlers.TryGetValue(commandType, out var handler) && handler != null)
        {
            return handler;
        }
        throw new NotImplementedException($"Missing command handler for {commandType.FullName}.");
    }

    internal IQueryHandler GetQueryHandlerFor(Type queryType)
    {
        if (_queryHandlers.TryGetValue(queryType, out var handler) && handler != null)
        {
            return handler;
        }
        throw new NotImplementedException($"Missing query handler for {queryType.FullName}");
    }

    public ICommandResponse Execute(ICommand command, Func<RequestStatus, bool>? shouldAddCommandToHistory = null)
    {
        var handler = GetCommandHandlerFor(command.GetType());
        var response = handler.Execute(command, this);
     
        if (shouldAddCommandToHistory != null && shouldAddCommandToHistory(response.Status))
        {
            AddCommandToHistory(command);
        }

        return response;
    }

    public ICommandResponse<TResponse>? Execute<TResponse>(ICommand<TResponse> command,
        Func<RequestStatus, bool>? shouldAddCommandToHistory = null)
    {
        var handler = GetCommandHandlerFor(command.GetType());
        var response = handler.Execute(command, this) as ICommandResponse<TResponse>;
        
        if (shouldAddCommandToHistory != null && shouldAddCommandToHistory(response!.Status))
        {
            AddCommandToHistory(command);
        }

        return response;
    }

    private void AddCommandToHistory(ICommand command)
    {
        if (_commandHistory.Count == CommandHistoryMaxSize)
        {
            _commandHistory.Remove(_commandHistory.First());
        }

        _commandHistory.Add(command);
        _redoHistory.Clear();
    }

    private void MoveLastCommandFromRedoHistoryToHistory()
    {
        if (_redoHistory.Count == 0)
        {
            return;
        }
        var command = _redoHistory.Last();
        _redoHistory.Remove(command);
        _commandHistory.Add(command);
    }

    public IQueryResponse<T>? Execute<T>(IQuery<T> query)
    {
        var handler = GetQueryHandlerFor(query.GetType());
        _logger.LogDebug("Executing query of type {0}", query.GetType().FullName);
        return handler.Execute(query) as IQueryResponse<T>;
    }

    public void Undo(ICommand command)
    {
        if (_commandHandlers.TryGetValue(command.GetType(), out var genericHandler) && genericHandler != null)
        {
            _logger.LogDebug("Undoing command of type {0}", command.GetType().FullName);
            genericHandler.Undo(command, this);
        }
        else
        {
            throw new NotImplementedException($"Missing command handler for {command.GetType().FullName}.");
        }
    }

    public bool UndoLastCommand()
    {
        if (_commandHistory.Count == 0)
        {
            return false;
        }

        var lastCommand = _commandHistory.Last();

        Undo(lastCommand);

        _commandHistory.Remove(lastCommand);
        _redoHistory.Add(lastCommand);
        return true;
    }

    public bool Redo()
    {
        if (_redoHistory.Count == 0) 
        {
            return false;
        }

        var lastCommandUndone = _redoHistory.Last();
        
        // we can't let Execute add it back to the history because that would prune the redo history,
        // which is not needed when redoing commands from the redo historys
        Execute(lastCommandUndone, (_) => false);
        MoveLastCommandFromRedoHistoryToHistory();
        
        return true;
    }

    public int HistoryLength => _commandHistory.Count;
}
