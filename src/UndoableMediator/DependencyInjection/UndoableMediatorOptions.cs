using System.Reflection;

namespace UndoableMediator.DependencyInjection;

public class UndoableMediatorOptions
{
    /// <summary>
    ///     The max amount of commands to remember for the mediator, once full, the oldest command will be removed from the history.
    ///     64 by default.
    /// </summary>
    public int CommandHistoryMaxSize { get; set; } = 64;

    /// <summary>
    ///     The max amount of commands to remember for the redo operation.
    ///     Will be cleared anytime a command is executed and added to the history since it rewrites the whole thing.
    ///     32 by default.
    /// </summary>
    public int RedoHistoryMaxSize { get; set; } = 32;
    
    /// <summary>
    ///     Additional assemblies that UndoableMediator should scan for handlers.
    ///     The way .NET Core loads assemblies means that sometimes your assembly won't be found by AppDomain.CurrentDomain.GetAssemblies().
    ///     In that case, specify the assembly in this array.
    /// </summary>
    public Assembly[] AssembliesToScan { get; set; } = Array.Empty<Assembly>();

    /// <summary>
    ///     When true, UndoableMediator will scan all assemblies found in AppDomain.CurrentDomain.GetAssemblies().
    ///     Defaults to false. When false, only assemblies specified in <see cref="AssembliesToScan"/> will be scanned.
    /// </summary>
    public bool ShouldScanAutomatically { get; set; }
}