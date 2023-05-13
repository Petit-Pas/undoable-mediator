using System.Reflection;
using Microsoft.Extensions.Logging;
using UndoableMediator.Commands;
using UndoableMediator.Queries;
using UndoableMediator.Requests;

namespace UndoableMediator.Mediators;

// TODO this still needs a sanity check after the ScanAssemblies

public class Mediator : IUndoableMediator
{
    private readonly ILogger _logger;

    private readonly Dictionary<Type, ICommandHandler?> _commandHandlers = new();
    private readonly Dictionary<Type, IQueryHandler?> _queryHandlers = new();

    // Config
    internal static int CommandHistoryMaxSize { get; set; } = 64;
    internal static Assembly[]? AdditionalAssemblies = Array.Empty<Assembly>();
    internal static bool ThrowsOnMissingHandler { get; set; }
    internal static bool ShouldScanAutomatically { get; set; }

    private readonly List<ICommand> _commandHistory;

    public Mediator(ILogger<Mediator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (CommandHistoryMaxSize <= 0)
        {
            throw new InvalidOperationException($"Cannot build an Mediator with a CommandHistoryMaxSize of {CommandHistoryMaxSize}");
        }
        _commandHistory = new List<ICommand>(CommandHistoryMaxSize);

        ScanAssemblies();
    }

    internal void ScanAssemblies()
    {
        Console.WriteLine("INFO: Mediator is scanning application for commands.");

        var assembliesToScan = ShouldScanAutomatically 
            ? AppDomain.CurrentDomain.GetAssemblies() 
            : Array.Empty<Assembly>();
        
        if (AdditionalAssemblies != null && AdditionalAssemblies.Length != 0)
        {
            assembliesToScan = assembliesToScan.Union(AdditionalAssemblies).ToArray();
        }

        foreach (var assembly in assembliesToScan)
        {
            Console.WriteLine($"DEBUG : MediatorBase is scanning '{assembly.FullName}' assembly looking for commands");
            try
            {
                foreach (var implementationType in assembly.GetTypes())
                {
                    var name = implementationType.FullName;
                    ;
                    if (!implementationType.GetTypeInfo().IsAbstract)
                    {
                        foreach (var interfaceType in implementationType.GetInterfaces())
                        {
                            if (interfaceType == typeof(ICommand))
                            {
                                Console.WriteLine(
                                    $"INFO : MediatorBase found the '{implementationType.FullName}' command.");
                                RegisterCommand(implementationType);
                            }
                            else if (interfaceType == typeof(IQuery))
                            {
                                Console.WriteLine(
                                    $"INFO : MediatorBase found the '{implementationType.FullName}' query.");
                                RegisterQuery(implementationType);
                            }
                            else if (interfaceType.IsGenericType &&
                                     interfaceType.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                            {
                                var commandType = interfaceType.GetGenericArguments().Single();
                                Console.WriteLine(
                                    $"INFO : MediatorBase found the '{implementationType.FullName}' command handler to handle {commandType.FullName}.");
                                RegisterCommandHandler(implementationType, commandType);
                            }
                            else if (interfaceType.IsGenericType &&
                                     interfaceType.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
                            {
                                var commandType = interfaceType.GetGenericArguments().First();
                                Console.WriteLine(
                                    $"INFO : MediatorBase found the '{implementationType.FullName}' command handler to handle {commandType.FullName}.");
                                RegisterCommandHandler(implementationType, commandType);
                            }
                            else if (interfaceType.IsGenericType &&
                                     interfaceType.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                            {
                                var queryType = interfaceType.GetGenericArguments().First();
                                Console.WriteLine(
                                    $"INFO : MediatorBase found the '{implementationType.FullName}' query handler to handle {queryType.FullName}.");
                                RegisterQueryHandler(implementationType, queryType);
                            }
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                Console.WriteLine($"WARNING : BaseMediator could not load types from {assembly.FullName}");
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
            Console.WriteLine(
                $"WARNING : will not register command {commandType.FullName} because it was already known by Mediator.");
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
            Console.WriteLine(
                $"WARNING : will not register query {queryType.FullName} because it was already known by Mediator.");
        }
    }

    internal void RegisterCommandHandler(Type commandHandlerType, Type commandType)
    {
        if (_commandHandlers.TryGetValue(commandType, out var registeredHandler))
        {
            if (registeredHandler != null)
            {
                Console.WriteLine(
                    $"WARNING : will override command handler {registeredHandler.GetType().FullName} with {commandHandlerType.FullName} " +
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
                Console.WriteLine(
                    $"WARNING : will override query handler {registeredHandler.GetType().FullName} with {queryHandlerType.FullName} " +
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
        var wasSuccessful = true;

        foreach (var (commandType, handlerType) in _commandHandlers)
        {
            if (handlerType == null)
            {
                _logger.LogWarning($"Did not find a corresponding handler for command {commandType.FullName}.");
                wasSuccessful = false;
            }
        }
        return wasSuccessful;
    }

    private bool HasSomeQueriesWithoutHandler()
    {
        var wasSuccessful = true;
        
        foreach (var (queryType, handlerType) in _queryHandlers)
        {
            if (handlerType == null)
            {
                _logger.LogWarning($"Did not find a corresponding handler for query {queryType.FullName}.");
                wasSuccessful = false;
            }
        }
        return wasSuccessful;
    }

    public ICommandResponse Execute(ICommand command, Func<RequestStatus, bool>? shouldAddCommandToHistory = null)
    {
        if (_commandHandlers.TryGetValue(command.GetType(), out var handler) && handler != null)
        {
            var response = handler.Execute(command, this);
            if (shouldAddCommandToHistory != null && shouldAddCommandToHistory(response.Status))
            {
                AddCommandToHistory(command);
            }

            return response;
        }

        throw new NullReferenceException($"ERROR : Missing command handler for {command.GetType().FullName}.");
    }

    public ICommandResponse<TResponse>? Execute<TResponse>(ICommand<TResponse> command,
        Func<RequestStatus, bool>? shouldAddCommandToHistory = null)
    {
        if (_commandHandlers.TryGetValue(command.GetType(), out var handler) && handler != null)
        {
            var response = handler.Execute(command, this) as ICommandResponse<TResponse>;
            if (shouldAddCommandToHistory != null && shouldAddCommandToHistory(response!.Status))
            {
                AddCommandToHistory(command);
            }

            return response;
        }

        throw new NullReferenceException($"ERROR : Missing command handler for {command.GetType().FullName}.");
    }

    private void AddCommandToHistory(ICommand command)
    {
        if (_commandHistory.Count == CommandHistoryMaxSize)
        {
            _commandHistory.Remove(_commandHistory.First());
        }

        _commandHistory.Add(command);
    }

    public IQueryResponse<T>? Execute<T>(IQuery<T> query)
    {
        if (_queryHandlers.TryGetValue(query.GetType(), out var handler) && handler != null)
        {
            return handler.Execute(query) as IQueryResponse<T>;
        }

        throw new NullReferenceException($"ERROR : Missing command handler for {query.GetType().FullName}.");
    }

    // At the moment, we undo the last command and that's it, no redo expected in first iteration
    public void Undo(ICommand command)
    {
        if (_commandHistory.Count == 0)
        {
            return;
        }

        var lastCommand = _commandHistory.Last();

        if (_commandHandlers.TryGetValue(command.GetType(), out var genericHandler) && genericHandler != null)
        {
            genericHandler.Undo(command, this);
        }
    }

    public void UndoLastCommand()
    {
        if (_commandHistory.Count == 0)
        {
            return;
        }

        var lastCommand = _commandHistory.Last();

        Undo(lastCommand);

        _commandHistory.Remove(lastCommand);
    }
}
