using System.Reflection;

namespace UndoableMediator.DependencyInjection;

public class UndoableMediatorOptions
{
    /// <summary>
    ///     The max amount of commands to remember for the mediator, once full, the oldest command will be removed from the history
    ///     64 by default.
    /// </summary>
    public int CommandHistoryMaxSize { get; set; } = 64;

    /// <summary>
    ///     The max amount of commands to remember for the redo operation.
    ///     Will be cleared anytime a command is executed and added to the history since it rewrites the whole thing.
    /// </summary>
    public int RedoHistoryMaxSize { get; set; } = 32;
    
    /// <summary>
    ///     UndoableMediator will do its best to find commands, however, the ways .net core loads assemblies makes it that sometime, your assembly won't be found by AppDomain.CurrentDomain.GetAssemblies();
    ///     In that case, you can just specify the assembly in that array.

    /// </summary>
    public Assembly[] AssembliesToScan { get; set; } = Array.Empty<Assembly>();

    /// <summary>
    ///     UndoableMediator will scan all assemblies it finds in AppDomain.CurrentDomain.GetAssemblies() by default.
    ///     This settings is used to prevent this comportment.
    ///     When used, you probably need to use AssembliesToScan as well.
    /// </summary>
    public bool ShouldScanAutomatically { get; set; }
}