using UndoableMediator.Requests;

namespace UndoableMediator.Commands;

/// <summary>
///     Base generic interface for command responses. Holds the response as well as the status
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface ICommandResponse<TResponse> : ICommandResponse
{
    /// <summary>
    ///     The potential response of the command
    /// </summary>
    public TResponse? Response { get; }

}

/// <summary>
///     Base interface for command response. Only holds status
/// </summary>
public interface ICommandResponse
{
    /// <summary>
    ///     The status of the command
    /// </summary>
    public RequestStatus Status { get; }
}

/// <summary>
///     Represents void for a commandResponse, so a command that returns nothing.
/// </summary>
public class NoResponse 
{ 
    public static readonly NoResponse Value = new NoResponse();

    private NoResponse() { }
} 