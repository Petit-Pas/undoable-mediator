using UndoableMediator.Commands;

namespace UndoableMediator.TestModels;

public class ChangeAgeAndNameCommand : CommandBase

{
    public ChangeAgeAndNameCommand(int age, string name) 	
    {
        Age = age;
        Name = name;
    }

    public int Age { get; }
    public string Name { get; }
}
