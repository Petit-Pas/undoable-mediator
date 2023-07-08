using Microsoft.Extensions.Logging;
using System.Reflection;
using UndoableMediator.Commands;
using UndoableMediator.DependencyInjection;
using UndoableMediator.Queries;
using UndoableMediator.Requests;

namespace UndoableMediator.Mediators;

public class Mediator : IUndoableMediator
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public Mediator(ILogger<IUndoableMediator> logger, IServiceProvider serviceProvider, UndoableMediatorOptions options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        _commandHistoryMaxSize = options.CommandHistoryMaxSize;
        _commandHistoryMaxSize = options.RedoHistoryMaxSize;

        _commandHistory = new List<ICommand>(_commandHistoryMaxSize);
        _redoHistory = new List<ICommand>(_commandRedoHistoryMaxSize);
    }

    // Config
    internal int _commandHistoryMaxSize { get; set; }
    internal int _commandRedoHistoryMaxSize { get; set; }

    // TODO these could be replaced with a max sized stack
    internal readonly List<ICommand> _commandHistory;
    internal readonly List<ICommand> _redoHistory;

    private ICommandHandler GetCommandHandlerFor<TResponse>(ICommand<TResponse> command)
    {
        var commandHandlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));

        var handler = _serviceProvider.GetService(commandHandlerType) as ICommandHandler;

        if (handler == null)
        {
            throw new NotImplementedException($"Missing command handler for {command.GetType().FullName}.");
        }

        return handler;
    }

    private IQueryHandler GetQueryHandlerFor<TResponse>(IQuery<TResponse> query)
    {
        var queryHandlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));

        var handler = _serviceProvider.GetService(queryHandlerType) as IQueryHandler;

        if (handler == null)
        {
            throw new NotImplementedException($"Missing query handler for {query.GetType().FullName}.");
        }

        return handler;
    }

    // <inheritdoc />
    public async Task<ICommandResponse<TResponse>> Execute<TResponse>(ICommand<TResponse> command, Func<RequestStatus, bool>? shouldAddCommandToHistory = null)
    {
        var handler = GetCommandHandlerFor(command);

        var response = await handler.Execute(command);

        if (shouldAddCommandToHistory != null && shouldAddCommandToHistory(response.Status))
        {
            AddCommandToHistory(command);
        }
        return (ICommandResponse<TResponse>)response;
    }


    // <inheritdoc />
    public async Task<IQueryResponse<TResponse>> Execute<TResponse>(IQuery<TResponse> query)
    {
        var handler = GetQueryHandlerFor(query);
        return (await handler.Execute(query)) as IQueryResponse<TResponse>;
    }

    // <inheritdoc />
    public void Undo(ICommand command)
    {
        ICommandHandler? handler = null;

        foreach (var interfaceType in command.GetType().GetInterfaces())
        {
            if (interfaceType.GetGenericTypeDefinition() == typeof(ICommand<>))
            {
                var methodInfo = GetPrivateMethodByReflection<Mediator>(nameof(GetCommandHandlerFor), 1, interfaceType.GenericTypeArguments[0]);
                try
                {
                    handler = methodInfo.Invoke(this, new object[] { command }) as ICommandHandler;
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException ?? e;
                }
                break;
            }
        }
        if (handler == null)
        {
            throw new NotImplementedException($"Could not find a handler for command of type {command.GetType().FullName}");
        }

        handler.Undo(command);
    }

    // <inheritdoc />
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

    // <inheritdoc />
    public async Task Redo(ICommand command)
    {
        ICommandHandler? handler = null;

        foreach (var interfaceType in command.GetType().GetInterfaces())
        {
            if (interfaceType.GetGenericTypeDefinition() == typeof(ICommand<>))
            {
                var methodInfo = GetPrivateMethodByReflection<Mediator>(nameof(GetCommandHandlerFor), 1, interfaceType.GenericTypeArguments[0]);
                try
                {
                    handler = methodInfo.Invoke(this, new object[] { command }) as ICommandHandler;
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException ?? e;
                }
                break;
            }
        }
        if (handler == null)
        {
            throw new NotImplementedException($"Could not find a handler for command of type {command.GetType().FullName}");
        }

        await handler.Redo(command);
    }

    // <inheritdoc />
    public async Task<bool> RedoLastUndoneCommand()
    {
        if (_redoHistory.Count == 0) 
        {
            return false;
        }

        var lastCommandUndone = _redoHistory.Last();

        await Redo(lastCommandUndone);

        MoveLastCommandFromRedoHistoryToHistory();
        
        return true;
    }

    private void AddCommandToHistory(ICommand command)
    {
        if (_commandHistory.Count == _commandHistoryMaxSize)
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

    public int HistoryLength => _commandHistory.Count;

    public int RedoHistoryLength => _redoHistory.Count;

    int IUndoableMediator.HistoryLength => throw new NotImplementedException();

    int IUndoableMediator.RedoHistoryLength => throw new NotImplementedException();

    //  These methods could be moved in another class.
    private MethodInfo GetClosedGenericMethod(MethodInfo baseMethod, Type type1)
    {
        return baseMethod.MakeGenericMethod(type1);
    }

    private MethodInfo GetClosedGenericMethod(MethodInfo baseMethod, Type type1, Type? type2 = null)
    {
        if (type2 == null)
        {
            return GetClosedGenericMethod(baseMethod, type1);
        }
        return baseMethod.MakeGenericMethod(type1, type2);
    }

    private MethodInfo GetPrivateMethodByReflection<TClass>(string methodName, int genericArguments, Type type1, Type? type2 = null)
    {
        var baseMethod = typeof(TClass).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(x => x.Name == methodName && x.GetGenericArguments().Length == genericArguments);
        return GetClosedGenericMethod(baseMethod, type1, type2);
    }
}
