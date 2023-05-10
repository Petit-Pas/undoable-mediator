using UndoableMediator.Requests;

namespace UndoableMediator.Commands;

public class CommandResponse<TResponse> : CommandResponse, ICommandResponse<TResponse>
{
    private CommandResponse(TResponse response, RequestStatus status) : base(status)
    {
        Response = response;
    }

    public static CommandResponse<TResponse> Canceled(TResponse response)
    {
        return new CommandResponse<TResponse>(response, RequestStatus.Canceled);
    }

    public static CommandResponse<TResponse> Success(TResponse response)
    {
        return new CommandResponse<TResponse>(response, RequestStatus.Success);
    }

    public static CommandResponse<TResponse> Failed(TResponse response)
    {
        return new CommandResponse<TResponse>(response, RequestStatus.Failed);
    }

    public TResponse? Response { get; }
}

public class CommandResponse : ICommandResponse
{
    internal CommandResponse(RequestStatus status)
    {
        Status = status;
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

    public RequestStatus Status { get; }
}