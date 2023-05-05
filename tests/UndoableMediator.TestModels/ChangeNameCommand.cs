using UndoableMediator.Commands;

namespace UndoableMediator.TestModels
{
    public class ChangeNameCommand : CommandBase
    {
        public ChangeNameCommand(string newName)
        {
            NewName = newName;
        }

        public string NewName { get; }
        public string? OldName { get; set; }
    }
}
