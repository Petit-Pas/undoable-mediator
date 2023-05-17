using System;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UndoableMediator.Mediators;
using UndoableMediator.MissingHandlerDll;

namespace UndoableMediator.Tests.Mediators;

[TestFixture]
public class MediatorTests
{
    [TestFixture]
    public class MissingHandlerTests
    {
        private ILogger<Mediator> _logger = null!;

        [SetUp]
        public void Setup()
        {
            _logger = A.Fake<ILogger<Mediator>>();

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
}