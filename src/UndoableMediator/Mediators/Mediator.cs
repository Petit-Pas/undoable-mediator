using Microsoft.Extensions.DependencyInjection;
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
    private int _commandHistoryMaxSize { get; set; }
    private int _commandRedoHistoryMaxSize { get; set; }

    // TODO these could be replaced with a max sized stack
    internal readonly List<ICommand> _commandHistory;
    internal readonly List<ICommand> _redoHistory;

    private ICommandHandler GetCommandHandlerFor<TCommand>()
        where TCommand : class, ICommand
    {
        var handler = _serviceProvider.GetService(typeof(ICommandHandler<TCommand>)) as ICommandHandler;

        if (handler == null)
        {
            throw new NotImplementedException($"Missing command handler for {typeof(TCommand).FullName}.");
        }

        return handler;
    }

    private ICommandHandler GetCommandHandlerFor<TCommand, TResponse>()
        where TCommand : class, ICommand<TResponse>
    {
        var handler = _serviceProvider.GetService(typeof(ICommandHandler<TCommand, TResponse>)) as ICommandHandler;

        if (handler == null)
        {
            throw new NotImplementedException($"Missing command handler for {typeof(TCommand).FullName}.");
        }

        return handler;
    }

    private IQueryHandler GetQueryHandlerFor<TQuery, TResponse>()
        where TQuery : IQuery<TResponse>
    {
        var handler = _serviceProvider.GetService(typeof(IQueryHandler<TQuery, TResponse>)) as IQueryHandler;

        if (handler == null)
        {
            throw new NotImplementedException($"Missing query handler for {typeof(TQuery).FullName}");
        }

        return handler;
    }

    public ICommandResponse Execute(ICommand command, ICommandHandler commandHandler, Func<RequestStatus, bool>? shouldAddCommandToHistory = null)
    {
        var response = commandHandler.Execute(command);

        if (shouldAddCommandToHistory != null && shouldAddCommandToHistory(response.Status))
        {
            AddCommandToHistory(command);
        }
        return response;
    }

    public ICommandResponse Execute<TCommand>(TCommand command, Func<RequestStatus, bool>? shouldAddCommandToHistory = null)
        where TCommand : class, ICommand
    {
        var handler = GetCommandHandlerFor<TCommand>();

        return Execute(command, handler, shouldAddCommandToHistory);
    }

    public ICommandResponse<TCommandResponse> Execute<TCommand, TCommandResponse>(TCommand command, Func<RequestStatus, bool>? shouldAddCommandToHistory = null)
        where TCommand : class, ICommand<TCommandResponse>
    {
        var handler = GetCommandHandlerFor<TCommand, TCommandResponse>();

        return (ICommandResponse<TCommandResponse>)Execute(command, handler, shouldAddCommandToHistory);
    }


    public IQueryResponse<TResponse>? Execute<TQuery, TResponse>(TQuery query)
        where TQuery : IQuery<TResponse>
    {
        var handler = GetQueryHandlerFor<TQuery, TResponse>();
        return handler.Execute(query) as IQueryResponse<TResponse>;
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

    public void Undo(ICommand command)
    {
        ICommandHandler? handler = null;

        foreach (var interfaceType in command.GetType().GetInterfaces())
        {
            if (interfaceType == typeof(ICommand))
            {
                var methodInfo = GetPrivateMethodByReflection<Mediator>(nameof(GetCommandHandlerFor), 1, command.GetType());
                handler = methodInfo.Invoke(this, null) as ICommandHandler;
                break;
            }
            else if (interfaceType.GetGenericTypeDefinition() == typeof(ICommand<>))
            {
                var methodInfo = GetPrivateMethodByReflection<Mediator>(nameof(GetCommandHandlerFor), 2, command.GetType(), interfaceType.GenericTypeArguments[0]);
                handler = methodInfo.Invoke(this, null) as ICommandHandler;
                break;
            }
        }
        if (handler == null)
        {
            throw new NotImplementedException($"Could not find a handler for command of type {command.GetType().FullName}");
        }

        handler.Undo(command);
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

        foreach (var interfaceType in lastCommandUndone.GetType().GetInterfaces())
        {
            if (interfaceType == typeof(ICommand))
            {
                var methodInfo = GetPrivateMethodByReflection<Mediator>(nameof(Execute), 1, lastCommandUndone.GetType());
                methodInfo.Invoke(this, new object[] {lastCommandUndone, new Func<RequestStatus, bool> ((_) => false)});
                break;
            }
            else if (interfaceType.GetGenericTypeDefinition() == typeof(ICommand<>))
            {
                var methodInfo = GetPrivateMethodByReflection<Mediator>(nameof(Execute), 2, lastCommandUndone.GetType(), interfaceType.GenericTypeArguments[0]);
                methodInfo.Invoke(this, new object[] { lastCommandUndone, new Func<RequestStatus, bool> ((_) => false) });
                break;
            }
        }

        MoveLastCommandFromRedoHistoryToHistory();
        
        return true;
    }

    public int HistoryLength => _commandHistory.Count;

    /// <summary>
    ///  These methods could be moved in another class.
    /// </summary>
    /// <param name="baseMethod"></param>
    /// <param name="type1"></param>
    /// <returns></returns>
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
