namespace UndoableMediator.Commands;

/// <summary>
///     Base generic interface for a command
/// </summary>
/// <typeparam name="TResponse"> The type of the response expected </typeparam>
public interface ICommand<TResponse> : ICommand
{
}

/// <summary>
///     The base interface for a command
/// </summary>
public interface ICommand
{
}

