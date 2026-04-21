namespace UndoableMediator.Requests;

/// <summary>
///     Helper to cast untyped request objects to their expected concrete types,
///     throwing a descriptive <see cref="InvalidOperationException"/> on mismatch.
/// </summary>
internal static class RequestCastHelper
{
    /// <summary>
    ///     Casts <paramref name="request"/> to <typeparamref name="TRequest"/>
    ///     or throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <param name="request"> The untyped request object </param>
    /// <param name="operation"> A verb describing the operation (e.g. "execute", "undo", "redo") for the error message </param>
    public static TRequest CastOrThrow<TRequest>(object request, string operation) where TRequest : class
    {
        return request as TRequest
            ?? throw new InvalidOperationException(
                $"Cannot {operation} request of type {request.GetType().FullName} because it is not of type {typeof(TRequest).FullName}");
    }
}
