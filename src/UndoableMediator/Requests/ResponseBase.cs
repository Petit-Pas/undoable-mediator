namespace UndoableMediator.Requests;

/// <summary>
///     Base class for command and query response types, holding common properties.
/// </summary>
/// <typeparam name="TResponse"> The type of the response </typeparam>
public abstract class ResponseBase<TResponse>
{
    protected ResponseBase(TResponse? response, RequestStatus status)
    {
        Response = response;
        Status = status;
    }

    /// <summary>
    ///     The potential response of the request
    /// </summary>
    public TResponse? Response { get; }

    /// <summary>
    ///     The status of the request
    /// </summary>
    public RequestStatus Status { get; }
}
