using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UndoableMediator.Commands;
using UndoableMediator.DependencyInjection;
using UndoableMediator.Mediators;
using UndoableMediator.MissingHandlerDll;
using UndoableMediator.Queries;
using UndoableMediator.Requests;
using UndoableMediator.TestModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UndoableMediator.Tests.Mediators;

[TestFixture]
public class MediatorTests
{
    private ILogger<Mediator> _logger = null!;
    private IServiceProvider _serviceProvider = null!;
    private UndoableMediatorOptions _options = null!;

    private Mediator _mediator = null!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _logger = A.Fake<ILogger<Mediator>>();
        _serviceProvider = A.Fake<IServiceProvider>();
        _options = new UndoableMediatorOptions()
        {
            AssembliesToScan = Array.Empty<Assembly>(),
            CommandHistoryMaxSize = 10,
            RedoHistoryMaxSize = 10,
            ShouldScanAutomatically = false,
        };

        _mediator = new Mediator(_logger, _serviceProvider, _options);
    }


    // Execute
    private Func<Task> ExecutingCommand<TResponse>(ICommand<TResponse> command)
    {
        return async () => await _mediator.Execute(command);
    }

    private Func<Task> ExecutingQuery<TResponse>(IQuery<TResponse> command)
    {
        return async () => await _mediator.Execute(command);
    }

    // Undo
    private Action UndoingCommand<TResponse>(ICommand<TResponse> command)
    {
        return () => _mediator.Undo(command);
    }

    // Redo
    private Func<Task> RedoingCommand<TResponse>(ICommand<TResponse> command)
    {
        return async () => await _mediator.Redo(command);
    }

    [TestFixture]
    public class MissingHandlerTests : MediatorTests
    {
        // Execute
        [Test]
        public void Should_Throw_Not_Implemented_Exception_When_Executing_An_Unknown_Command()
        {
            // Arrange
            A.CallTo(() => _serviceProvider.GetService(typeof(ICommandHandler<ChangeAgeCommand, NoResponse>)))
                .Returns(null);

            // Act & Assert
            ExecutingCommand(new ChangeAgeCommand(12))
                .Should()
                .ThrowAsync<NotImplementedException>();
        }

        [Test]
        public void Should_Throw_Not_Implemented_Exception_When_Executing_An_Unknown_Query()
        {
            // Arrange
            A.CallTo(() => _serviceProvider.GetService(typeof(IQueryHandler<CancelableQuery, bool>)))
                .Returns(null);

            // Act & Assert
            ExecutingQuery(new CancelableQuery(true))
                .Should()
                .ThrowAsync<NotImplementedException>();
        }

        // Undo
        [Test]
        public void Should_Throw_Not_Implemented_Exception_When_Undoing_An_Unknown_Command()
        {
            // Arrange
            A.CallTo(() => _serviceProvider.GetService(typeof(ICommandHandler<ChangeAgeCommand, NoResponse>)))
                .Returns(null);

            // Act & Assert
            UndoingCommand(new ChangeAgeCommand(12))
                .Should()
                .Throw<NotImplementedException>();
        }

        // Redo
        [Test]
        public void Should_Throw_Not_Implemented_Exception_When_Redoing_An_Unknown_Command()
        {
            // Arrange
            A.CallTo(() => _serviceProvider.GetService(typeof(ICommandHandler<ChangeAgeCommand, NoResponse>)))
                .Returns(null);

            // Act & Assert
            RedoingCommand(new ChangeAgeCommand(12))
                .Should()
                .ThrowAsync<NotImplementedException>();
        }
    }

    [TestFixture]
    public class ExecuteCommandTests : MediatorTests
    {
        ICommandHandler<CancelableCommand, bool> _commandHandler = null!;
        CancelableCommand _command = null!;

        RequestStatus _requestStatus = RequestStatus.Success;
        bool _requestAnswer = true;

        [SetUp]
        public void Setup()
        {
            _commandHandler = A.Fake<ICommandHandler<CancelableCommand, bool>>();
            _command = new CancelableCommand(false);

            A.CallTo(() => _serviceProvider.GetService(typeof(ICommandHandler<CancelableCommand, bool>)))
                .Returns(_commandHandler);
            A.CallTo(() => _commandHandler.Execute(A<ICommand<bool>>._))
                .ReturnsLazily(() => new CommandResponse<bool>(_requestAnswer, _requestStatus));

        }

        [Test]
        public void Should_Call_Execute_Method_Of_Handler()
        {
            // Arrange
            // Act
            _mediator.Execute(_command);

            // Assert
            A.CallTo(() => _commandHandler.Execute(A<ICommand<bool>>._))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        [TestCase(RequestStatus.Success, false)]
        [TestCase(RequestStatus.Canceled, true)]
        public async Task Should_Return_CommandStatus(RequestStatus expectedRequestStatus, bool commandShouldBeCanceled)
        {
            // Arrange
            _requestStatus = expectedRequestStatus;

            // Act
            var result = await _mediator.Execute(_command);

            // Assert
            result.Status.Should().Be(expectedRequestStatus);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public async Task Should_Return_Command_Response(bool commandShouldBeCanceled)
        {
            // Arrange
            _requestAnswer = commandShouldBeCanceled;

            // Act
            var result = await _mediator.Execute(_command);

            // Assert
            result.Response.Should().Be(commandShouldBeCanceled);
        }

        [Test]
        [TestCase(null, 0)]
        [TestCase(false, 0)]
        [TestCase(true, 1)]
        public async Task Should_Add_To_History_When_Requested_Only(bool? shouldAdd, int addedToHistory)
        {
            // Arrange
            var previousHistorySize = _mediator.HistoryLength;

            // Act
            await _mediator.Execute(_command, shouldAdd != null ? (_) => shouldAdd.Value : null);

            // Assert
            _mediator.HistoryLength.Should().Be(previousHistorySize + addedToHistory);
        }

        [Test]
        [TestCase(RequestStatus.Success)]
        [TestCase(RequestStatus.Failed)]
        [TestCase(RequestStatus.Canceled)]
        public void Should_Provide_Request_Status_To_AddToHistory_Delegate(RequestStatus requestStatus)
        {
            // Arrange
            _requestStatus = requestStatus;

            // Act & Assert
            _mediator.Execute(_command, (status) =>
            {
                status.Should().Be(requestStatus);
                return true;
            });
        }

        [Test]
        public async Task Should_Roll_Out_A_Command_From_History_When_Max_Limit_Has_Been_Reached()
        {
            while (_mediator.HistoryLength != _mediator._commandHistoryMaxSize)
            {
                _mediator._commandHistory.Add(new CancelableCommand(false));
            }
            var lastCommand = _mediator._commandHistory.First();
            var nextToLastCommand = _mediator._commandHistory[1];

            // Act
            await _mediator.Execute(new CancelableCommand(false), _ => true);

            // Assert
            _mediator._commandHistory.First().Should().Be(nextToLastCommand);
            _mediator._commandHistory.Should().NotContain(lastCommand);
        }

        [Test]
        public async Task Should_Erase_All_Possible_Redo_History()
        {
            // Arrange
            _mediator._redoHistory.Add(new CancelableCommand(true));
            _mediator._redoHistory.Add(new CancelableCommand(true));
            _mediator._redoHistory.Add(new CancelableCommand(true));

            // Act
            await _mediator.Execute(_command, _ => true);

            // Assert
            _mediator.RedoHistoryLength.Should().Be(0);
        }
    }

    [TestFixture]
    public class ExecuteQueryTests : MediatorTests
    {
        IQueryHandler<CancelableQuery, bool> _queryHandler = null!;
        CancelableQuery _query = null!;

        RequestStatus _requestStatus = RequestStatus.Success;
        bool _requestAnswer = true;

        [SetUp]
        public void Setup()
        {
            _queryHandler = A.Fake<IQueryHandler<CancelableQuery, bool>>();
            _query = new CancelableQuery();

            A.CallTo(() => _serviceProvider.GetService(typeof(IQueryHandler<CancelableQuery, bool>)))
                .Returns(_queryHandler);

            A.CallTo(() => _queryHandler.Execute(A<IQuery<bool>>._))
                .ReturnsLazily(() => new QueryResponse<bool>(_requestAnswer, _requestStatus));
        }

        [Test]
        public async Task Should_Call_Execute_Method_Of_Handler()
        {
            // Arrange
            // Act
            await _mediator.Execute(_query);

            // Assert
            A.CallTo(() => _queryHandler.Execute(A<IQuery<bool>>._))
                .MustHaveHappened();
        }

        [Test]
        [TestCase(RequestStatus.Success, false)]
        [TestCase(RequestStatus.Canceled, true)]
        public async Task Should_Return_CommandStatus(RequestStatus expectedRequestStatus, bool commandShouldBeCanceled)
        {
            // Arrange
            _requestStatus = expectedRequestStatus;

            // Act
            var result = await _mediator.Execute(_query);

            // Assert
            result.Status.Should().Be(expectedRequestStatus);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public async Task Should_Return_Command_Response(bool queryShouldBeCanceled)
        {
            // Arrange
            _requestAnswer = queryShouldBeCanceled;

            // Act
            var result = await _mediator.Execute(_query);

            // Assert
            result.Response.Should().Be(queryShouldBeCanceled);
        }
    }

    [TestFixture]
    public class UndoCommandTests : MediatorTests
    {
        ICommandHandler<CancelableCommand, bool> _commandHandler = null!;
        CancelableCommand _command = null!;

        [SetUp]
        public void Setup()
        {
            _commandHandler = A.Fake<ICommandHandler<CancelableCommand, bool>>();
            _command = new CancelableCommand(false);

            A.CallTo(() => _serviceProvider.GetService(typeof(ICommandHandler<CancelableCommand, bool>)))
                .Returns(_commandHandler);
        }

        [Test]
        public void Should_Call_Undo_Method_Of_Handler()
        {
            // Arrange
            // Act
            _mediator.Undo(_command);

            // Assert
            A.CallTo(() => _commandHandler.Undo(A<ICommand<bool>>._))
                .MustHaveHappenedOnceExactly();
        }
    }

    [TestFixture]
    public class UndoLastCommandTests : MediatorTests
    {
        CancelableCommand _lastAddedCommand = null!;

        [SetUp]
        public async Task Setup()
        {
            var commandHandler = A.Fake<ICommandHandler<CancelableCommand, bool>>();

            A.CallTo(() => _serviceProvider.GetService(typeof(ICommandHandler<CancelableCommand, bool>)))
                .Returns(commandHandler);
            A.CallTo(() => commandHandler.Execute(A<ICommand<bool>>._))
                .ReturnsLazily(() => new CommandResponse<bool>(true, RequestStatus.Success));


            await _mediator.Execute(new CancelableCommand(false), _ => true);
            await _mediator.Execute(new CancelableCommand(false), _ => true);
            await _mediator.Execute(new CancelableCommand(false), _ => true);

            _lastAddedCommand = (CancelableCommand)_mediator._commandHistory.Last();
        }

        [Test]
        public void Should_Return_False_When_History_Is_Empty()
        {
            // Arrange
            _mediator._commandHistory.Clear();

            // Act
            var result = _mediator.UndoLastCommand();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void Should_Return_True_When_History_Is_Not_Empty()
        {
            // Act
            var result = _mediator.UndoLastCommand();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void Should_Move_Command_From_History_To_Redo_History_When_Undoing()
        {
            // Act
            _mediator.UndoLastCommand();

            // Assert
            _mediator._commandHistory.Should().NotContain(_lastAddedCommand);
            _mediator._redoHistory.Should().Contain(_lastAddedCommand);
        }
    }

    [TestFixture]
    public class RedoCommandTests : MediatorTests
    {
        ICommandHandler<CancelableCommand, bool> _commandHandler = null!;
        CancelableCommand _command = null!;

        [SetUp]
        public void Setup()
        {
            _commandHandler = A.Fake<ICommandHandler<CancelableCommand, bool>>();
            _command = new CancelableCommand(false);

            A.CallTo(() => _serviceProvider.GetService(typeof(ICommandHandler<CancelableCommand, bool>)))
                .Returns(_commandHandler);
        }

        [Test]
        public void Should_Call_Redo_Method_Of_Handler()
        {
            // Arrange
            // Act
            _mediator.Redo(_command);

            // Assert
            A.CallTo(() => _commandHandler.Redo(A<ICommand<bool>>._))
                .MustHaveHappenedOnceExactly();
        }
    }

    [TestFixture]
    public class RedoLastUndoneCommandTests : MediatorTests
    {
        CancelableCommand _lastUndoneCommand = null!;

        [SetUp]
        public void Setup()
        {
            var commandHandler = A.Fake<ICommandHandler<CancelableCommand, bool>>();

            A.CallTo(() => _serviceProvider.GetService(typeof(ICommandHandler<CancelableCommand, bool>)))
                .Returns(commandHandler);
            A.CallTo(() => commandHandler.Execute(A<ICommand<bool>>._))
                .ReturnsLazily(() => new CommandResponse<bool>(true, RequestStatus.Success));


            _mediator.Execute(new CancelableCommand(false), _ => true);
            _mediator.Execute(new CancelableCommand(false), _ => true);
            _mediator.Execute(new CancelableCommand(false), _ => true);
            _mediator.UndoLastCommand();
            _mediator.UndoLastCommand();
            _mediator.UndoLastCommand();

            _lastUndoneCommand = (CancelableCommand)_mediator._redoHistory.Last();
        }

        [Test]
        public async Task Should_Return_False_When_History_Is_Empty()
        {
            // Arrange
            _mediator._redoHistory.Clear();

            // Act
            var result = await _mediator.RedoLastUndoneCommand();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void Should_Return_True_When_History_Is_Not_Empty()
        {
            // Act
            var result = _mediator.UndoLastCommand();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Should_Move_Command_From_Redo_History_To_History_When_Undoing()
        {
            // Act
            await _mediator.RedoLastUndoneCommand();

            // Assert
            _mediator._commandHistory.Should().Contain(_lastUndoneCommand);
            _mediator._redoHistory.Should().NotContain(_lastUndoneCommand);
        }
    }
}