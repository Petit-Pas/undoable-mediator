namespace UndoableMediator.Requests;

/// <summary>
///     Represents the final status of command/query
/// </summary>
public enum RequestStatus
{
    Success,
    Failed,
    Canceled
}