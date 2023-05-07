namespace UndoableMediator.Queries;

public interface IQueryResponse<T>
{
    T? Response { get; }
    bool WasCanceled { get; }
}
