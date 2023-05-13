using UndoableMediator.Commands;

namespace UndoableMediator.TestModels;

public class ChangeAgeCommand : CommandBase
{
	public ChangeAgeCommand(int newAge)
	{
		NewAge = newAge;
	}

	public int NewAge { get; }
	public int OldAge { get; set; }
}
