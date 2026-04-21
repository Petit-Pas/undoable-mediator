using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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


    // Send
    private Func<Task> SendingCommand<TResponse>(ICommand<TResponse> command)
    {
        return async () => await _mediator.SendAsync(command);
    }

    private Func<Task> QueryingQuery<TResponse>(IQuery<TResponse> query)
    {
        return async () => await _mediator.QueryAsync(query);
    }

    // Undo
    private Func<Task> UndoingCommand<TResponse>(ICommand<TResponse> command)
    {
        return async () => await ((ISubCommandDispatcher)_mediator).UndoAsync(command);
    }

    // Redo
    private Func<Task> RedoingCommand<TResponse>(ICommand<TResponse> command)
    {
        return async () => await ((ISubCommandDispatcher)_mediator).RedoAsync(command);
    }

    [TestFixture]
    public class MissingHandlerTests : MediatorTests
    {
        // Send
        [Test]
        public void Should_Throw_Not_Implemented_Exception_When_Sending_An_Unknown_Command()
        {
            // Arrange
            A.CallTo(() => _serviceProvider.GetService(typeof(ICommandHandler<ChangeAgeCommand, NoResponse>)))
                .Returns(null);

            // Act & Assert
            SendingCommand(new ChangeAgeCommand(12))
                .Should()
                .ThrowAsync<NotImplementedException>();
        }

        [Test]
        public void Should_Throw_Not_Implemented_Exception_When_Querying_An_Unknown_Query()
        {
            // Arrange
            A.CallTo(() => _serviceProvider.GetService(typeof(IQueryHandler<CancelableQuery, bool>)))
                .Returns(null);

            // Act & Assert
            QueryingQuery(new CancelableQuery(true))
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
                .ThrowAsync<NotImplementedException>();
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
    public class SendCommandTests : MediatorTests
    {
        ICommandHandler<CancelableCommand, bool> _commandHandler = null!;
        CancelableCommand _command = null!;

        RequestStatus _requestStatus = RequestStatus.Success;
        bool _requestAnswer = true;

        [SetUp]
        public void Setup()
        {
            _mediator.CommandHistory.Clear();
            _mediator.RedoHistory.Clear();

            _requestStatus = RequestStatus.Success;
            _requestAnswer = true;

            _commandHandler = A.Fake<ICommandHandler<CancelableCommand, bool>>();
            _command = new CancelableCommand(false);

            A.CallTo(() => _serviceProvider.GetService(typeof(ICommandHandler<CancelableCommand, bool>)))
                .Returns(_commandHandler);
            A.CallTo(() => _commandHandler.ExecuteAsync(A<ICommand<bool>>._))
                .ReturnsLazily(() => new CommandResponse<bool>(_requestAnswer, _requestStatus));

        }

        [Test]
        public void Should_Call_ExecuteAsync_Method_Of_Handler()
        {
            // Arrange
            // Act
            _mediator.SendAsync(_command);

            // Assert
            A.CallTo(() => _commandHandler.ExecuteAsync(A<ICommand<bool>>._))
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
            var result = await _mediator.SendAsync(_command);

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
            var result = await _mediator.SendAsync(_command);

            // Assert
            result.Response.Should().Be(commandShouldBeCanceled);
        }

        [Test]
        public async Task Should_Add_To_History_On_Success_Only()
        {
            // Arrange
            var previousHistorySize = _mediator.HistoryLength;

            // Act — success response (default)
            _requestStatus = RequestStatus.Success;
            await _mediator.SendAsync(_command);

            // Assert
            _mediator.HistoryLength.Should().Be(previousHistorySize + 1);
        }

        [Test]
        [TestCase(RequestStatus.Failed)]
        [TestCase(RequestStatus.Canceled)]
        public async Task Should_Not_Add_To_History_On_Non_Success(RequestStatus status)
        {
            // Arrange
            var previousHistorySize = _mediator.HistoryLength;
            _requestStatus = status;

            // Act
            await _mediator.SendAsync(_command);

            // Assert
            _mediator.HistoryLength.Should().Be(previousHistorySize);
        }

        [Test]
        public async Task Should_Roll_Out_A_Command_From_History_When_Max_Limit_Has_Been_Reached()
        {
            while (_mediator.HistoryLength != _mediator.CommandHistoryMaxSize)
            {
                _mediator.CommandHistory.AddLast(new CancelableCommand(false));
            }
            var lastCommand = _mediator.CommandHistory.First!.Value;
            var nextToLastCommand = _mediator.CommandHistory.Skip(1).First();

            // Act
            await _mediator.SendAsync(new CancelableCommand(false));
            _mediator.CommandHistory.First!.Value.Should().Be(nextToLastCommand);
            _mediator.CommandHistory.Should().NotContain(lastCommand);
        }

        [Test]
        public async Task Should_Erase_All_Possible_Redo_History()
        {
            // Arrange
            _mediator.RedoHistory.AddLast(new CancelableCommand(true));
            _mediator.RedoHistory.AddLast(new CancelableCommand(true));
            _mediator.RedoHistory.AddLast(new CancelableCommand(true));

            // Act
            await _mediator.SendAsync(_command);

            // Assert
            _mediator.RedoHistoryLength.Should().Be(0);
        }
    }

    [TestFixture]
    public class QueryTests : MediatorTests
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

            A.CallTo(() => _queryHandler.ExecuteAsync(A<IQuery<bool>>._))
                .ReturnsLazily(() => new QueryResponse<bool>(_requestAnswer, _requestStatus));
        }

        [Test]
        public async Task Should_Call_ExecuteAsync_Method_Of_Handler()
        {
            // Arrange
            // Act
            await _mediator.QueryAsync(_query);

            // Assert
            A.CallTo(() => _queryHandler.ExecuteAsync(A<IQuery<bool>>._))
                .MustHaveHappened();
        }

        [Test]
        [TestCase(RequestStatus.Success, false)]
        [TestCase(RequestStatus.Canceled, true)]
        public async Task Should_Return_QueryStatus(RequestStatus expectedRequestStatus, bool queryShouldBeCanceled)
        {
            // Arrange
            _requestStatus = expectedRequestStatus;

            // Act
            var result = await _mediator.QueryAsync(_query);

            // Assert
            result.Status.Should().Be(expectedRequestStatus);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public async Task Should_Return_Query_Response(bool queryShouldBeCanceled)
        {
            // Arrange
            _requestAnswer = queryShouldBeCanceled;

            // Act
            var result = await _mediator.QueryAsync(_query);

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
        public async Task Should_Call_UndoAsync_Method_Of_Handler()
        {
            // Arrange
            // Act
            await ((ISubCommandDispatcher)_mediator).UndoAsync(_command);

            // Assert
            A.CallTo(() => _commandHandler.UndoAsync(A<ICommand<bool>>._))
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
            A.CallTo(() => commandHandler.ExecuteAsync(A<ICommand<bool>>._))
                .ReturnsLazily(() => new CommandResponse<bool>(true, RequestStatus.Success));


            await _mediator.SendAsync(new CancelableCommand(false));
            await _mediator.SendAsync(new CancelableCommand(false));
            await _mediator.SendAsync(new CancelableCommand(false));

            _lastAddedCommand = (CancelableCommand)_mediator.CommandHistory.Last!.Value;
        }

        [Test]
        public async Task Should_Return_False_When_History_Is_Empty()
        {
            // Arrange
            _mediator.CommandHistory.Clear();

            // Act
            var result = await _mediator.UndoLastCommandAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Should_Return_True_When_History_Is_Not_Empty()
        {
            // Act
            var result = await _mediator.UndoLastCommandAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Should_Move_Command_From_History_To_Redo_History_When_Undoing()
        {
            // Act
            await _mediator.UndoLastCommandAsync();

            // Assert
            _mediator.CommandHistory.Should().NotContain(_lastAddedCommand);
            _mediator.RedoHistory.Should().Contain(_lastAddedCommand);
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
        public async Task Should_Call_RedoAsync_Method_Of_Handler()
        {
            // Arrange
            // Act
            await ((ISubCommandDispatcher)_mediator).RedoAsync(_command);

            // Assert
            A.CallTo(() => _commandHandler.RedoAsync(A<ICommand<bool>>._))
                .MustHaveHappenedOnceExactly();
        }
    }

    [TestFixture]
    public class RedoLastUndoneCommandTests : MediatorTests
    {
        CancelableCommand _lastUndoneCommand = null!;

        [SetUp]
        public async Task Setup()
        {
            var commandHandler = A.Fake<ICommandHandler<CancelableCommand, bool>>();

            A.CallTo(() => _serviceProvider.GetService(typeof(ICommandHandler<CancelableCommand, bool>)))
                .Returns(commandHandler);
            A.CallTo(() => commandHandler.ExecuteAsync(A<ICommand<bool>>._))
                .ReturnsLazily(() => new CommandResponse<bool>(true, RequestStatus.Success));


            await _mediator.SendAsync(new CancelableCommand(false));
            await _mediator.SendAsync(new CancelableCommand(false));
            await _mediator.SendAsync(new CancelableCommand(false));
            await _mediator.UndoLastCommandAsync();
            await _mediator.UndoLastCommandAsync();
            await _mediator.UndoLastCommandAsync();

            _lastUndoneCommand = (CancelableCommand)_mediator.RedoHistory.Last!.Value;
        }

        [Test]
        public async Task Should_Return_False_When_History_Is_Empty()
        {
            // Arrange
            _mediator.RedoHistory.Clear();

            // Act
            var result = await _mediator.RedoLastUndoneCommandAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task Should_Return_True_When_History_Is_Not_Empty()
        {
            // Act
            var result = await _mediator.RedoLastUndoneCommandAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task Should_Move_Command_From_Redo_History_To_History_When_Redoing()
        {
            // Act
            await _mediator.RedoLastUndoneCommandAsync();

            // Assert
            _mediator.CommandHistory.Should().Contain(_lastUndoneCommand);
            _mediator.RedoHistory.Should().NotContain(_lastUndoneCommand);
        }

        [Test]
        public async Task Should_Respect_Max_History_Size_When_Moving_Redone_Command_To_History()
        {
            // Fill history to max
            _mediator.CommandHistory.Clear();
            for (int i = 0; i < _mediator.CommandHistoryMaxSize; i++)
            {
                _mediator.CommandHistory.AddLast(new CancelableCommand(false));
            }
            var oldestCommand = _mediator.CommandHistory.First!.Value;

            // Act
            await _mediator.RedoLastUndoneCommandAsync();

            // Assert
            _mediator.HistoryLength.Should().Be(_mediator.CommandHistoryMaxSize);
            _mediator.CommandHistory.Should().NotContain(oldestCommand);
        }
    }

    [TestFixture]
    public class NullArgumentTests : MediatorTests
    {
        [Test]
        public void Should_Throw_ArgumentNullException_When_Sending_Null_Command()
        {
            Func<Task> act = async () => await _mediator.SendAsync<bool>(null!);
            act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public void Should_Throw_ArgumentNullException_When_Querying_Null_Query()
        {
            Func<Task> act = async () => await _mediator.QueryAsync<bool>(null!);
            act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public void Should_Throw_ArgumentNullException_When_Undoing_Null_Command()
        {
            Func<Task> act = async () => await ((ISubCommandDispatcher)_mediator).UndoAsync(null!);
            act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Test]
        public void Should_Throw_ArgumentNullException_When_Redoing_Null_Command()
        {
            Func<Task> act = async () => await ((ISubCommandDispatcher)_mediator).RedoAsync(null!);
            act.Should().ThrowAsync<ArgumentNullException>();
        }
    }

    [TestFixture]
    public class SubCommandOrderTests : MediatorTests
    {
        [Test]
        public async Task Should_Undo_SubCommands_In_Reverse_Execution_Order()
        {
            // Arrange
            var fakeInnerMediator = A.Fake<IUndoableMediator>(o => o.Implements(typeof(ISubCommandDispatcher)));
            var fakeDispatcher = (ISubCommandDispatcher)fakeInnerMediator;
            var handler = new CancelableCommandHandler(fakeInnerMediator);

            var command = new CancelableCommand(false);
            var firstSubCommand = new ChangeAgeCommand(10);
            var secondSubCommand = new ChangeNameCommand("Alice");
            ((ISubCommandHost)command).AddSubCommand(firstSubCommand);   // executed first → bottom of stack
            ((ISubCommandHost)command).AddSubCommand(secondSubCommand);  // executed second → top of stack

            var undoOrder = new List<ICommand>();
            A.CallTo(() => fakeDispatcher.UndoAsync(A<ICommand>._))
                .Invokes((ICommand c) => undoOrder.Add(c));

            // Act
            await ((ICommandHandler)handler).UndoAsync(command);

            // Assert: second sub-command undone first (reverse execution order)
            undoOrder.Should().ContainInOrder(secondSubCommand, firstSubCommand);
        }

        [Test]
        public async Task Should_Redo_SubCommands_In_Original_Execution_Order()
        {
            // Arrange
            var fakeInnerMediator = A.Fake<IUndoableMediator>(o => o.Implements(typeof(ISubCommandDispatcher)));
            var fakeDispatcher = (ISubCommandDispatcher)fakeInnerMediator;
            var handler = new CancelableCommandHandler(fakeInnerMediator);

            var command = new CancelableCommand(false);
            var firstSubCommand = new ChangeAgeCommand(10);
            var secondSubCommand = new ChangeNameCommand("Alice");
            ((ISubCommandHost)command).AddSubCommand(firstSubCommand);   // executed first → bottom of stack
            ((ISubCommandHost)command).AddSubCommand(secondSubCommand);  // executed second → top of stack

            var redoOrder = new List<ICommand>();
            A.CallTo(() => fakeDispatcher.RedoAsync(A<ICommand>._))
                .Invokes((ICommand c) => redoOrder.Add(c));

            // Act
            await ((ICommandHandler)handler).RedoAsync(command);

            // Assert: first sub-command redone first (original execution order)
            redoOrder.Should().ContainInOrder(firstSubCommand, secondSubCommand);
        }
    }
}