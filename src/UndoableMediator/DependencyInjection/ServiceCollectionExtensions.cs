using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using UndoableMediator.Commands;
using UndoableMediator.Mediators;
using UndoableMediator.Queries;

namespace UndoableMediator.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void ConfigureMediator(this IServiceCollection serviceCollection, Action<UndoableMediatorOptions>? optionSetter = null, ILoggerFactory? loggerFactory = null)
    {
        var options = new UndoableMediatorOptions();

        if (optionSetter != null)
        {
            optionSetter(options);
        }

        if (options.CommandHistoryMaxSize <= 0)
        {
            throw new InvalidOperationException($"Cannot configure a Mediator with a CommandHistoryMaxSize of {options.CommandHistoryMaxSize}");
        }
        if (options.RedoHistoryMaxSize <= 0)
        {
            throw new InvalidOperationException($"Cannot configure a Mediator with a RedoHistoryMaxSize of {options.RedoHistoryMaxSize}");
        }

        var logger = loggerFactory?.CreateLogger(typeof(ServiceCollectionExtensions)) ?? NullLogger.Instance;

        serviceCollection.AddSingleton(options);
        serviceCollection.AddSingleton<IUndoableMediator, Mediator>();

        ScanAssemblies(options.AssembliesToScan, options.ShouldScanAutomatically, serviceCollection, logger);
    }

    internal static void ScanAssemblies(Assembly[] additionalAssemblies, bool shouldScanAutomatically, IServiceCollection serviceCollection, ILogger logger)
    {
        var assembliesToScan = shouldScanAutomatically
            ? AppDomain.CurrentDomain.GetAssemblies()
            : Array.Empty<Assembly>();

        if (additionalAssemblies != null && additionalAssemblies.Length != 0)
        {
            assembliesToScan = assembliesToScan.Union(additionalAssemblies).ToArray();
        }

        foreach (var assembly in assembliesToScan)
        {
            logger.LogDebug("Mediator is scanning '{AssemblyName}' assembly looking for handlers to register.", assembly.FullName);
            try
            {
                foreach (var implementationType in assembly.GetTypes().Where(t => !t.GetTypeInfo().IsAbstract))
                {
                    foreach (var interfaceType in implementationType.GetInterfaces().Where(i => i.IsGenericType))
                    {
                        var genericDefinition = interfaceType.GetGenericTypeDefinition();
                        if (genericDefinition == typeof(ICommandHandler<,>) ||
                            genericDefinition == typeof(IQueryHandler<,>))
                        {
                            serviceCollection.AddTransient(interfaceType, implementationType);
                            logger.LogDebug("Mediator found handler '{HandlerType}' to register.", implementationType);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                logger.LogWarning("UndoableMediator could not load types from {AssemblyName}", assembly.FullName);
            }
        }
    }
}

