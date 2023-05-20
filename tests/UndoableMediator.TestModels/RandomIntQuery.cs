using UndoableMediator.Queries;

namespace UndoableMediator.TestModels;

public class RandomIntQuery : QueryBase<int>
{
	public RandomIntQuery(int range = 1000)
	{
        Range = range;
    }

    public int Range { get; }
}
