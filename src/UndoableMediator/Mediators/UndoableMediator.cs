using System.Data;
using System.Reflection;
using UndoableMediator.Commands;
using UndoableMediator.Queries;

namespace UndoableMediator.Mediators;

public class UndoableMediator : IUndoableMediator
{
    private readonly Dictionary<Type, ICommandHandler?> _commandHandlers = new ();
    private readonly Dictionary<Type, IQueryHandler?> _queryHandlers = new ();

    /// <summary>
    ///     if a keyword list is provided, UndoableMediator will only be looking for requests and handlers in assemblies that contain one of those keywords
    /// </summary>
    /// <param name="keywords"></param>
    public UndoableMediator(IEnumerable<string>? keywords = null)
    {
        ScanAssemblies(keywords?.ToArray());
    }

    private void ScanAssemblies(string[]? keywords)
    {
        Console.WriteLine("INFO: UndoableMediator is scanning application for commands.");

        var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies();
        if (keywords != null && keywords.Length != 0)
        {
            assembliesToScan = assembliesToScan.Where(a => keywords.Any(k => a.FullName?.Contains(k) ?? false)).ToArray();
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
                                Console.WriteLine($"INFO : MediatorBase found the '{implementationType.FullName}' command.");
                                RegisterCommand(implementationType);
                            }
                            else if (interfaceType == typeof(IQuery))
                            {
                                Console.WriteLine($"INFO : MediatorBase found the '{implementationType.FullName}' query.");
                                RegisterQuery(implementationType);
                            }
                            else if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                            {
                                var commandType = interfaceType.GetGenericArguments().Single();
                                Console.WriteLine($"INFO : MediatorBase found the '{implementationType.FullName}' command handler to handle {commandType.FullName}.");
                                RegisterCommandHandler(implementationType, commandType);
                            }
                            else if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
                            {
                                var commandType = interfaceType.GetGenericArguments().First();
                                Console.WriteLine($"INFO : MediatorBase found the '{implementationType.FullName}' command handler to handle {commandType.FullName}.");
                                RegisterCommandHandler(implementationType, commandType);
                            }
                            else if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IQueryHandler<,>))
                            {
                                var queryType = interfaceType.GetGenericArguments().First();
                                Console.WriteLine($"INFO : MediatorBase found the '{implementationType.FullName}' query handler to handle {queryType.FullName}.");
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

    private void RegisterCommand(Type commandType)
    {
        if (!_commandHandlers.ContainsKey(commandType))
        {
            _commandHandlers.Add(commandType, null);
        }
        else
        {
            Console.WriteLine($"WARNING : will not register command {commandType.FullName} because it was already known by UndoableMediator.");
        }
    }

    private void RegisterQuery(Type queryType)
    {
        if (!_queryHandlers.ContainsKey(queryType))
        {
            _queryHandlers.Add(queryType, null);
        }
        else
        {
            Console.WriteLine($"WARNING : will not register query {queryType.FullName} because it was already known by UndoableMediator.");
        }
    }

    private void RegisterCommandHandler(Type commandHandlerType, Type commandType)
    {
        if (_commandHandlers.TryGetValue(commandType, out var registeredHandler))
        {
            if (registeredHandler != null)
            {
                Console.WriteLine($"WARNING : will override command handler {registeredHandler.GetType().FullName} with {commandHandlerType.FullName} " +
                                  $"as the handler for {commandType.FullName} since a new value was found by UndoableMediator");
            }
        }
        _commandHandlers[commandType] = Activator.CreateInstance(commandHandlerType) as ICommandHandler;
    }

    private void RegisterQueryHandler(Type queryHandlerType, Type queryType)
    {
        if (_queryHandlers.TryGetValue(queryType, out var registeredHandler))
        {
            if (registeredHandler != null)
            {
                Console.WriteLine($"WARNING : will override query handler {registeredHandler.GetType().FullName} with {queryHandlerType.FullName} " +
                                  $"as the handler for {queryType.FullName} since a new value was found by UndoableMediator");
            }
        }
        _queryHandlers[queryType] = Activator.CreateInstance(queryHandlerType) as IQueryHandler;
    }

    
    public void Execute(ICommand command, bool addToHistory = false)
    {
        if (_commandHandlers.TryGetValue(command.GetType(), out var handler) && handler != null)
        {
            handler.Execute(command, this);
        }
        else
        {
            throw new NullReferenceException($"ERROR : Missing command handler for {command.GetType().FullName}.");
        }
    }

    public CommandResponse<TResponse>? Execute<TResponse>(ICommand<TResponse> command, bool addToHistory = false)
    {
        if (_commandHandlers.TryGetValue(command.GetType(), out var handler) && handler != null)
        {
            return handler.Execute(command, this) as CommandResponse<TResponse>;
        }
        else
        {
            throw new NullReferenceException($"ERROR : Missing command handler for {command.GetType().FullName}.");
        }
    }

    public IQueryResponse<T>? Execute<T>(IQuery<T> query)
    {
        if (_queryHandlers.TryGetValue(query.GetType(), out var handler) && handler != null)
        {
            return handler.Execute(query) as IQueryResponse<T>;
        }
        throw new NullReferenceException($"ERROR : Missing command handler for {query.GetType().FullName}.");
    }

    public void Undo(ICommand command)
    {
        throw new NotImplementedException();
    }
}