using UndoableMediator.Queries;
using UndoableMediator.Requests;

namespace UndoableMediator.Commands;

/// <summary>
///     Generic class for command responses
/// </summary>
/// <typeparam name="TResponse"> The type of the response </typeparam>
public class CommandResponse<TResponse> : ICommandResponse<TResponse>
{
    internal CommandResponse(TResponse? response, RequestStatus status)
    {
        Response = response;
        Status = status;
    }

    public CommandResponse(ICommandResponse commandResponse) : this(default, commandResponse.Status)
    {
    }

    public CommandResponse(IQueryResponse commandResponse) : this(default, commandResponse.Status)
    {
    }

    // <inheritdoc />
    public TResponse? Response { get; }

    // <inheritdoc />
    public RequestStatus Status { get; }
}

/// <summary>
///     Can be used as a shortcut for a CommandResponse with no content
///     Also contains static utilities to build CommandResponses, with or without content
/// </summary>
public class CommandResponse : CommandResponse<NoResponse>
{
    internal CommandResponse(RequestStatus status) : base(NoResponse.Value, status)
    {
    }

    public CommandResponse(ICommandResponse commandResponse) : base(NoResponse.Value, commandResponse.Status)
    {
    }

    public CommandResponse(IQueryResponse commandResponse) : base(NoResponse.Value, commandResponse.Status)
    {
    }

    /// <summary>
    ///     Creates a CommandResponse with a status of Canceled and the given content
    /// </summary>
    /// <typeparam name="TContent"> Type expected as answer from the command </typeparam>
    /// <param name="response"> Answer of the command, 'default' by default </param>
    /// <returns> The build QueryResponse </returns>
    public static ICommandResponse<TContent> Canceled<TContent>(TContent? response = default)
    {
        return new CommandResponse<TContent>(response, RequestStatus.Canceled);
    }

    /// <summary>
    ///     Creates a CommandResponse with a status of Success and the given content
    /// </summary>
    /// <typeparam name="TContent"> Type expected as answer from the command </typeparam>
    /// <param name="response"> Answer of the command, 'default' by default </param>
    /// <returns> The build QueryResponse </returns>
    public static ICommandResponse<TContent> Success<TContent>(TContent? response = default)
    {
        return new CommandResponse<TContent>(response, RequestStatus.Success);
    }

    /// <summary>
    ///     Creates a CommandResponse with a status of Failed and the given content
    /// </summary>
    /// <typeparam name="TContent"> Type expected as answer from the command </typeparam>
    /// <param name="response"> Answer of the command, 'default' by default </param>
    /// <returns> The build QueryResponse </returns>
    public static ICommandResponse<TContent> Failed<TContent>(TContent? response = default)
    {
        return new CommandResponse<TContent>(response, RequestStatus.Failed);
    }

    /// <summary>
    ///     Creates a CommandResponse with a status of Canceled and a content of NoAnswer
    /// </summary>
    /// <returns> The build QueryResponse </returns>
    public static ICommandResponse Canceled()
    {
        return new CommandResponse(RequestStatus.Canceled);
    }

    /// <summary>
    ///     Creates a CommandResponse with a status of Success and a content of NoAnswer
    /// </summary>
    /// <returns> The build QueryResponse </returns>
    public static ICommandResponse<NoResponse> Success()
    {
        return new CommandResponse(RequestStatus.Success);
    }

    /// <summary>
    ///     Creates a CommandResponse with a status of Failed and a content of NoAnswer
    /// </summary>
    /// <returns> The build QueryResponse </returns>
    public static ICommandResponse Failed()
    {
        return new CommandResponse(RequestStatus.Failed);
    }
}