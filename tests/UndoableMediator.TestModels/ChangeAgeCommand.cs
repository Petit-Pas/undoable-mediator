using UndoableMediator.Commands;

namespace UndoableMediator.TestModels;

public class ChangeAgeCommand : CommandBase
{
	public ChangeAgeCommand(int newAge)
	{
		NewAge = newAge;
	}

    public ChangeAgeCommand()
    {
		// TODO should be removed, was for testing
        NewAge = 10;
    }

	public int NewAge { get; }
	public int OldAge { get; set; }
}
