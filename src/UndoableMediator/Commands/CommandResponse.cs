using UndoableMediator.Requests;

namespace UndoableMediator.Commands;

public class CommandResponse<T> : CommandResponse
{
    private CommandResponse(T response, RequestStatus status) : base(status)
    {
        Response = response;
    }

    public static CommandResponse<T> Canceled(T response)
    {
        return new CommandResponse<T>(response, RequestStatus.Canceled);
    }

    public static CommandResponse<T> Success(T response)
    {
        return new CommandResponse<T>(response, RequestStatus.Success);
    }

    public static CommandResponse<T> Failed(T response)
    {
        return new CommandResponse<T>(response, RequestStatus.Failed);
    }

    public T? Response { get; }
}

public class CommandResponse
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