using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Reflection;
using UndoableMediator.Commands;
using UndoableMediator.Mediators;
using UndoableMediator.Queries;

namespace UndoableMediator.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void ConfigureMediator(this IServiceCollection serviceCollection, Action<UndoableMediatorOptions>? optionSetter = null)
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


        serviceCollection.AddSingleton(options);
        serviceCollection.AddSingleton<IUndoableMediator, Mediator>();


        ScanAssemblies(options.AssembliesToScan, options.ShouldScanAutomatically, serviceCollection);
    }

    internal static void ScanAssemblies(Assembly[] AdditionalAssemblies, bool ShouldScanAutomatically, IServiceCollection serviceCollection)
    {
        var assembliesToScan = ShouldScanAutomatically
            ? AppDomain.CurrentDomain.GetAssemblies()
            : Array.Empty<Assembly>();

        if (AdditionalAssemblies != null && AdditionalAssemblies.Length != 0)
        {
            assembliesToScan = assembliesToScan.Union(AdditionalAssemblies).ToArray();
        }

        foreach (var assembly in assembliesToScan)
        {
            Console.WriteLine($"Mediator is scanning '{assembly.FullName}' assembly looking for handlers to register.");
            try
            {
                foreach (var implementationType in assembly.GetTypes().Where(t => !t.GetTypeInfo().IsAbstract))
                {
                    foreach (var interfaceType in implementationType.GetInterfaces().Where(i => i.IsGenericType))
                    {
                        var genericDefinition = interfaceType.GetGenericTypeDefinition();
                        if (genericDefinition == typeof(ICommandHandler<>) ||
                            genericDefinition == typeof(ICommandHandler<,>) ||
                            genericDefinition == typeof(IQueryHandler<,>))
                        {
                            serviceCollection.AddTransient(interfaceType, implementationType);
                            Console.WriteLine($"Mediator found handler '{implementationType}' to register.");
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                Console.WriteLine($"UndoableMediator could not load types from {assembly.FullName}");
            }
        }
    }
}

