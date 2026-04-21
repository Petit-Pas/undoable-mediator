using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Reflection;
using System.Threading.Tasks;
using UndoableMediator.Commands;
using UndoableMediator.DependencyInjection;
using UndoableMediator.Mediators;
using UndoableMediator.TestModels;

namespace UndoableMediator.Tests.Mediators;

/// <summary>
///     Integration fixture that demonstrates redo via <c>ClearSubCommands + re-execute</c>
///     instead of replaying the recorded sub-command tree.
///
///     The handler clears its sub-command stack and calls <c>ExecuteAsync</c> again on redo,
///     which re-runs the full logic from scratch (including re-dispatching sub-commands).
///
///     Model initial state:  Age = 10, Name = "default", Score = 0
///     Command arguments:    Age = 25, Name = "Alice",   Score = 100
///
///     Tests are ordered and share state: each test picks up where the previous left off.
/// </summary>
[TestFixture]
public class ReExecuteOnRedoTests
{
    private const int InitialAge = 10;
    private const string InitialName = "default";
    private const int InitialScore = 0;

    private const int NewAge = 25;
    private const string NewName = "Alice";
    private const int NewScore = 100;

    private Mediator _mediator = null!;
    private ReExecutingChangeAgeNameAndScoreCommand _command = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        AffectedObject.Age = InitialAge;
        AffectedObject.Name = InitialName;
        AffectedObject.Score = InitialScore;

        var logger = A.Fake<ILogger<IUndoableMediator>>();
        var serviceProvider = A.Fake<IServiceProvider>();
        var options = new UndoableMediatorOptions
        {
            AssembliesToScan = Array.Empty<Assembly>(),
            CommandHistoryMaxSize = 10,
            RedoHistoryMaxSize = 10,
            ShouldScanAutomatically = false,
        };

        _mediator = new Mediator(logger, serviceProvider, options);

        A.CallTo(() => serviceProvider.GetService(typeof(ICommandHandler<ReExecutingChangeAgeNameAndScoreCommand, NoResponse>)))
            .ReturnsLazily(() => new ReExecutingChangeAgeNameAndScoreCommandHandler(_mediator));

        A.CallTo(() => serviceProvider.GetService(typeof(ICommandHandler<ChangeAgeCommand, NoResponse>)))
            .ReturnsLazily(() => new ChangeAgeCommandHandler(_mediator));

        A.CallTo(() => serviceProvider.GetService(typeof(ICommandHandler<ChangeNameCommand, NoResponse>)))
            .ReturnsLazily(() => new ChangeNameCommandHandler(_mediator));

        _command = new ReExecutingChangeAgeNameAndScoreCommand(NewAge, NewName, NewScore);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        AffectedObject.Age = InitialAge;
        AffectedObject.Name = InitialName;
        AffectedObject.Score = InitialScore;
    }

    [Test, Order(1)]
    public async Task ExecuteAsync_Should_Mutate_All_Properties()
    {
        // Act
        await _mediator.SendAsync(_command);

        // Assert
        AffectedObject.Age.Should().Be(NewAge);
        AffectedObject.Name.Should().Be(NewName);
        AffectedObject.Score.Should().Be(NewScore);
    }

    [Test, Order(2)]
    public async Task UndoLastCommandAsync_Should_Restore_All_Properties()
    {
        // Act
        var undone = await _mediator.UndoLastCommandAsync();

        // Assert
        undone.Should().BeTrue();
        AffectedObject.Age.Should().Be(InitialAge);
        AffectedObject.Name.Should().Be(InitialName);
        AffectedObject.Score.Should().Be(InitialScore);
    }

    [Test, Order(3)]
    public async Task RedoLastUndoneCommandAsync_Should_Reapply_All_Properties_Via_ReExecute()
    {
        // Act
        var redone = await _mediator.RedoLastUndoneCommandAsync();

        // Assert
        redone.Should().BeTrue();
        AffectedObject.Age.Should().Be(NewAge);
        AffectedObject.Name.Should().Be(NewName);
        AffectedObject.Score.Should().Be(NewScore);
    }

    [Test, Order(4)]
    public async Task RedoLastUndoneCommandAsync_Should_Not_Accumulate_SubCommands_Across_Redo()
    {
        // Arrange: undo again so we can redo a second time
        await _mediator.UndoLastCommandAsync();

        var subCommandCountAfterFirstExecute = ((ISubCommandHost)_command).SubCommands.Count;

        // Act: redo — ClearSubCommands + re-execute must not stack on top of existing sub-commands
        await _mediator.RedoLastUndoneCommandAsync();

        // Assert: sub-command count is the same as after the first execute (not doubled)
        ((ISubCommandHost)_command).SubCommands.Count.Should().Be(subCommandCountAfterFirstExecute);
    }
}
