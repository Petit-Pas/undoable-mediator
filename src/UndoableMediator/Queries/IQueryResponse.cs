namespace UndoableMediator.Queries;

public interface IQueryResponse<T>
{
    T? Response { get; set; }
}
