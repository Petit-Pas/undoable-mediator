using UndoableMediator.Requests;

namespace UndoableMediator.Commands;

public interface ICommandResponse<TResponse> : ICommandResponse
{
    TResponse? Response { get; }
}

public interface ICommandResponse
{
    RequestStatus Status { get; }
}

/// <summary>
///     Represents void for a commandResponse
/// </summary>
public class NoResponse 
{ 
    public static readonly NoResponse Value = new NoResponse();

    private NoResponse() { }
} 