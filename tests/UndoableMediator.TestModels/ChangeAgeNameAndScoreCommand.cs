using UndoableMediator.Commands;

namespace UndoableMediator.TestModels;

public class ChangeAgeNameAndScoreCommand : CommandBase
{
    public ChangeAgeNameAndScoreCommand(int age, string name, int score)
    {
        Age = age;
        Name = name;
        Score = score;
    }

    public int Age { get; }
    public string Name { get; }
    public int Score { get; }

    public int OldScore { get; set; }
}
