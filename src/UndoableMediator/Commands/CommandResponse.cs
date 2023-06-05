using UndoableMediator.Requests;

namespace UndoableMediator.Commands;

public class CommandResponse<TResponse> : ICommandResponse<TResponse>
{
    internal CommandResponse(TResponse? response, RequestStatus status)
    {
        Response = response;
        Status = status;
    }
    
    public TResponse? Response { get; }

    public RequestStatus Status { get; }
}

/// <summary>
///     Only serves the purpose of containing build method to ease the client code
/// </summary>
public class CommandResponse : CommandResponse<NoResponse>
{
    private CommandResponse(RequestStatus status) : base(NoResponse.Value, status)
    {
    }

    public static CommandResponse<TContent> Canceled<TContent>(TContent? response = default)
    {
        return new CommandResponse<TContent>(response, RequestStatus.Canceled);
    }

    public static CommandResponse<TContent> Success<TContent>(TContent? response = default)
    {
        return new CommandResponse<TContent>(response, RequestStatus.Success);
    }

    public static CommandResponse<TContent> Failed<TContent>(TContent? response = default)
    {
        return new CommandResponse<TContent>(response, RequestStatus.Failed);
    }

    public static CommandResponse Canceled()
    {
        return new CommandResponse(RequestStatus.Canceled);
    }

    public static CommandResponse Success()
    {
        return new CommandResponse(RequestStatus.Success);
    }

    public static CommandResponse Failed()
    {
        return new CommandResponse(RequestStatus.Failed);
    }
}