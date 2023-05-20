using System;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UndoableMediator.Commands;
using UndoableMediator.Mediators;
using UndoableMediator.MissingHandlerDll;
using UndoableMediator.Requests;
using UndoableMediator.TestModels;

namespace UndoableMediator.Tests.Mediators;

[TestFixture]
public class MediatorTests
{
    private ILogger<Mediator> _logger = null!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _logger = A.Fake<ILogger<Mediator>>();
    }

    [TestFixture]
    public class MissingHandlerTests : MediatorTests
    {
        [SetUp]
        public void Setup()
        {
            Mediator.ShouldScanAutomatically = false;
            Mediator.AdditionalAssemblies = new[] { typeof(CommandWithoutHandler).Assembly };
            Mediator.ThrowsOnMissingHandler = false;
        }

        [Test]
        public void Should_Only_Warn_When_ThrowsOnMissingHandler_Is_Set_To_False()
        {
            // Arrange
            var building = () => _ = new Mediator(_logger);

            // Act & Assert
            building.Should().NotThrow();
        }

        [Test]
        public void Should_Throw_When_ThrowsOnMissingHandler_Is_Set_To_True()
        {
            // Arrange
            Mediator.ThrowsOnMissingHandler = true;
            var building = () => _ = new Mediator(_logger);

            // Act & Assert
            building.Should().Throw<NotImplementedException>();
        }

        [Test]
        public void Should_Throw_When_Trying_To_Execute_A_Command_That_Has_No_Handler()
        {
            // Arrange
            var mediator = new Mediator(_logger);
            var command = new CommandWithoutHandler();
            var executingCommand = () => mediator.Execute(command);

            // Act & Assert
            executingCommand.Should().Throw<NotImplementedException>();
        }

        [Test]
        public void Should_Throw_When_Trying_To_Execute_A_Returning_Command_That_Has_No_Handler()
        {
            // Arrange
            var mediator = new Mediator(_logger);
            var command = new ReturningCommandWithoutHandler();
            var executingCommand = () => mediator.Execute(command);

            // Act & Assert
            executingCommand.Should().Throw<NotImplementedException>();
        }

        [Test]
        public void Should_Throw_When_Trying_To_Execute_A_Query_That_Has_No_Handler()
        {
            // Arrange
            var mediator = new Mediator(_logger);
            var query = new QueryWithoutHandler();
            var executingQuery = () => mediator.Execute(query);

            // Act & Assert
            executingQuery.Should().Throw<NotImplementedException>();
        }

        [Test]
        public void Should_Throw_When_Trying_To_Undo_A_Command_That_Has_No_Handler()
        {
            // Arrange
            var mediator = new Mediator(_logger);
            var command = new CommandWithoutHandler();
            var undoingCommand = () => mediator.Undo(command);

            // Act & Assert
            undoingCommand.Should().Throw<NotImplementedException>();
        }

        [Test]
        public void Should_Throw_When_Trying_To_Undo_A_Returning_Command_That_Has_No_Handler()
        {
            // Arrange
            var mediator = new Mediator(_logger);
            var command = new ReturningCommandWithoutHandler();
            var undoingCommand = () => mediator.Undo(command);

            // Act & Assert
            undoingCommand.Should().Throw<NotImplementedException>();
        }

    }

    [TestFixture]
    public class QueryExecuteTests : MediatorTests
    {
        private IUndoableMediator _mediator = null!;

        [SetUp]
        public void SetUp()
        {
            Mediator.ShouldScanAutomatically = false;
            Mediator.AdditionalAssemblies = new[] { typeof(AffectedObject).Assembly };
            Mediator.ThrowsOnMissingHandler = true;

            _mediator = new Mediator(_logger);
        }

        [Test]
        public void Should_Return_A_Query_Response_Success_When_The_Query_Succeeds()
        {
            // Arrange
            var query = new CancelableQuery();

            // Act
            var result = _mediator.Execute(query);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be(RequestStatus.Success);
        }

        [Test]
        public void Should_Return_Cancel_When_The_Query_Is_Canceled()
        {
            // Arrange
            var query = new CancelableQuery(true);

            // Act
            var result = _mediator.Execute(query);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be(RequestStatus.Canceled);
        }

        [Test]
        public void Should_Return_Result_When_Query_Succeeds()
        {
            // Arrange
            var query = new CancelableQuery();

            // Act
            var result = _mediator.Execute(query);

            // Assert
            result.Should().NotBeNull();
            result!.Response.Should().BeTrue();
        }
    }

    [TestFixture]
    public class SimpleCommandExecuteTests : MediatorTests
    {
        private IUndoableMediator _mediator = null!;

        [SetUp]
        public void SetUp()
        {
            Mediator.ShouldScanAutomatically = false;
            Mediator.AdditionalAssemblies = new[] { typeof(AffectedObject).Assembly };
            Mediator.ThrowsOnMissingHandler = true;

            _mediator = new Mediator(_logger);
        }

        [Test]
        public void Should_Return_A_Command_Response_Success_When_The_Command_Succeeds()
        {
            // Arrange
            var command = new CancelableCommand();

            // Act
            var result = _mediator.Execute(command);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be(RequestStatus.Success);
        }

        [Test]
        public void Should_Return_Cancel_When_The_Command_Is_Canceled()
        {
            // Arrange
            var command = new CancelableCommand(true);

            // Act
            var result = _mediator.Execute(command);

            // Assert
            result.Should().NotBeNull();
            result!.Status.Should().Be(RequestStatus.Canceled);
        }

        [Test]
        public void Should_Return_Result_When_Command_Succeeds()
        {
            // Arrange
            var command = new SetRandomAgeCommand();

            // Act
            var result = _mediator.Execute(command);

            // Assert
            result.Should().NotBeNull();
            result!.Response.Should().Be(AffectedObject.Age);
        }

        [Test]
        public void Should_Have_An_Effect()
        {
            // Arrange
            var previousAge = AffectedObject.Age;
            var command = new ChangeAgeCommand(previousAge + 10);

            // Act
            _mediator.Execute(command);

            // Assert
            AffectedObject.Age.Should().Be(previousAge + 10);
        }
    }

    [TestFixture]
    public class ComplexCommandExecuteTests : MediatorTests
    {
        private IUndoableMediator _mediator = null!;

        [SetUp]
        public void SetUp()
        {
            Mediator.ShouldScanAutomatically = false;
            Mediator.AdditionalAssemblies = new[] { typeof(AffectedObject).Assembly };
            Mediator.ThrowsOnMissingHandler = true;

            _mediator = new Mediator(_logger);
        }

        [Test]
        public void Should_Set_SubCommands_Of_Parent_Command()
        {
            // Arrange
            var command = new SetRandomAgeCommand();

            // Act
            _mediator.Execute(command);

            // Assert
            command.SubCommands.Should().Contain(x => x.GetType() == typeof(ChangeAgeCommand));
        }
    }

    [TestFixture]
    public class SimpleCommandUndoTests : MediatorTests
    {
        private IUndoableMediator _mediator = null!;

        [SetUp]
        public void SetUp()
        {
            Mediator.ShouldScanAutomatically = false;
            Mediator.AdditionalAssemblies = new[] { typeof(AffectedObject).Assembly };
            Mediator.ThrowsOnMissingHandler = true;

            _mediator = new Mediator(_logger);
        }

        [Test]
        public void Should_Call_The_Undo_Method_On_The_Handler()
        {
            // Arrange
            var previousName = AffectedObject.Name;
            var newName = $"Longer {previousName}";
            var command = new ChangeNameCommand(newName);
            _mediator.Execute(command);
            AffectedObject.Name.Should().Be(newName);

            // Act
            _mediator.Undo(command);

            // Assert
            AffectedObject.Name.Should().Be(previousName);
        }
    }

    [TestFixture]
    public class ComplexCommandUndoTests : MediatorTests
    {
        private IUndoableMediator _mediator = null!;

        [SetUp]
        public void SetUp()
        {
            Mediator.ShouldScanAutomatically = false;
            Mediator.AdditionalAssemblies = new[] { typeof(AffectedObject).Assembly };
            Mediator.ThrowsOnMissingHandler = true;

            _mediator = new Mediator(_logger);
        }

        [Test]
        public void Should_Call_The_Undo_Method_On_The_SubCommands()
        {
            // Arrange
            var previousName = AffectedObject.Name;
            var newName = $"Longer {previousName}";
            var previousAge = AffectedObject.Age;
            var newAge = previousAge + 10;
            var command = new ChangeAgeAndNameCommand(newAge, newName);
            _mediator.Execute(command);
            AffectedObject.Name.Should().Be(newName);
            AffectedObject.Age.Should().Be(newAge);

            // Act
            _mediator.Undo(command);

            // Assert
            AffectedObject.Name.Should().Be(previousName);
            AffectedObject.Age.Should().Be(previousAge);
        }
    }

    [TestFixture]
    public class MediatorHistoryTests : MediatorTests
    {
        private Mediator _mediator = null!;

        [SetUp]
        public void SetUp()
        {
            Mediator.ShouldScanAutomatically = false;
            Mediator.AdditionalAssemblies = new[] { typeof(AffectedObject).Assembly };
            Mediator.ThrowsOnMissingHandler = true;

            _mediator = new Mediator(_logger);
        }

        [Test]
        public void Should_Not_Add_Command_To_History_When_Provider_Returns_False()
        {
            // Arrange
            var command = new ChangeAgeCommand(10);

            // Act
            _mediator.Execute(command);

            // Assert
            _mediator.HistoryLength.Should().Be(0);
        }

        [Test]
        public void Should_Add_Command_To_History_When_Provider_Returns_True()
        {
            // Arrange
            var command = new ChangeAgeCommand(10);

            // Act
            _mediator.Execute(command, _ => true);

            // Assert
            _mediator.HistoryLength.Should().Be(1);
        }

        [Test]
        [TestCase(false, 1)]
        [TestCase(true, 0)]
        public void Should_Use_Provider_To_Determine_When_A_Command_Needs_To_Be_Added_To_History(bool canceled, int expectedHistorySize)
        {
            // Arrange
            var command = new CancelableCommand(canceled);

            // Act
            _mediator.Execute(command, response => response is RequestStatus.Success);

            // Assert
            _mediator.HistoryLength.Should().Be(expectedHistorySize);
        }

        [Test]
        public void Should_Cycle_Commands_In_History_When_MaxSize_Is_Reached()
        {
            // Arrange
            while (_mediator.HistoryLength != Mediator.CommandHistoryMaxSize) 
            {
                _mediator._commandHistory.Add(new ChangeAgeCommand(12));
            }
            var lastCommand = _mediator._commandHistory.First();
            var nextToLastCommand = _mediator._commandHistory[1];

            // Act
            _mediator.Execute(new ChangeAgeCommand(12), _ => true);

            // Assert
            _mediator._commandHistory.First().Should().Be(nextToLastCommand);
            _mediator._commandHistory.Should().NotContain(lastCommand);
        }

        [Test]
        public void Should_Undo_Last_Command_Added_To_History()
        {
            // Arrange
            var previousAge = AffectedObject.Age;
            var registeredCommand = new ChangeAgeCommand(previousAge + 10);
            var nonRegisteredCommand = new ChangeAgeCommand(previousAge + 20);
            _mediator.Execute(registeredCommand, _ => true);
            _mediator.Execute(nonRegisteredCommand);

            // Act
            _mediator.UndoLastCommand();

            // Assert
            AffectedObject.Age.Should().Be(previousAge);
        }

        [Test]
        public void Undo_Should_Add_Command_To_Redo_History()
        {
            // Arrange
            var command = new ChangeAgeCommand(10);
            _mediator.Execute(command, _ => true);

            // Act
            _mediator.UndoLastCommand();

            // Arrange
            _mediator._redoHistory.Count.Should().Be(1);
        }

        [Test]
        public void Should_Redo_Last_Undone_Command()
        {
            // Arrange
            var previousAge = AffectedObject.Age;
            var firstCommand = new ChangeAgeCommand(previousAge + 10);
            var secondCommand = new ChangeAgeCommand(previousAge + 20);
            _mediator.Execute(firstCommand, _ => true);
            _mediator.Execute(secondCommand, _ => true);
            _mediator.UndoLastCommand();
            _mediator.UndoLastCommand();

            // Act
            _mediator.Redo();

            AffectedObject.Age.Should().Be(previousAge + 10);
        }
    }
}