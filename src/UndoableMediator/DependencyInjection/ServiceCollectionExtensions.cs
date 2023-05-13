using Microsoft.Extensions.DependencyInjection;
using UndoableMediator.Mediators;

namespace UndoableMediator.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddUndoableMediator(this IServiceCollection serviceCollection, Action<UndoableMediatorOptions>? optionSetter = null)
    {
        var options = new UndoableMediatorOptions();

        if (optionSetter != null)
        {
            optionSetter(options);
        }

        Mediator.CommandHistoryMaxSize = options.CommandHistoryMaxSize;
        Mediator.AdditionalAssemblies = options.AssembliesToScan;
        Mediator.ThrowsOnMissingHandler = options.ThrowsOnMissingHandler;
        Mediator.ShouldScanAutomatically = options.ShouldScanAutomatically;

        serviceCollection.AddSingleton<IUndoableMediator, Mediator>();
    }
}