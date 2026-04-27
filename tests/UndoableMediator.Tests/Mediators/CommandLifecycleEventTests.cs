using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UndoableMediator.Commands;
using UndoableMediator.DependencyInjection;
using UndoableMediator.Mediators;
using UndoableMediator.Queries;
using UndoableMediator.TestModels;

namespace UndoableMediator.Tests.Mediators;

/// <summary>
///     Verifies that OnCommandExecuted, OnCommandUndone, and OnCommandRedone are raised
///     only for top-level commands (not for sub-commands).
/// </summary>
[TestFixture]
public class CommandLifecycleEventTests
{
    private Mediator _mediator = null!;

    [SetUp]
    public void SetUp()
    {
        AffectedObject.Age = 10;
        AffectedObject.Name = "default";
        AffectedObject.Score = 0;

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

        // Register handlers for simple and composite commands
        A.CallTo(() => serviceProvider.GetService(typeof(ICommandHandler<ChangeAgeCommand, NoResponse>)))
            .ReturnsLazily(() => new ChangeAgeCommandHandler(_mediator));

        A.CallTo(() => serviceProvider.GetService(typeof(ICommandHandler<ChangeAgeAndNameCommand, NoResponse>)))
            .ReturnsLazily(() => new ChangeAgeAndNameCommandHandler(_mediator));

        A.CallTo(() => serviceProvider.GetService(typeof(ICommandHandler<ChangeNameCommand, NoResponse>)))
            .ReturnsLazily(() => new ChangeNameCommandHandler(_mediator));

        A.CallTo(() => serviceProvider.GetService(typeof(ICommandHandler<CancelableCommand, bool>)))
            .ReturnsLazily(() => new CancelableCommandHandler(_mediator));

        A.CallTo(() => serviceProvider.GetService(typeof(IQueryHandler<CancelableQuery, bool>)))
            .ReturnsLazily(() => new CancelableQueryHandler());
    }

    // ──────────────────────────────────────────────
    //  OnCommandExecuted
    // ──────────────────────────────────────────────

    [Test]
    public async Task OnCommandExecuted_Is_Raised_Once_For_TopLevel_Command()
    {
        var raised = new List<ICommand>();
        _mediator.OnCommandExecuted += (_, cmd) => raised.Add(cmd);

        var command = new ChangeAgeCommand(42);
        await _mediator.SendAsync(command);

        raised.Should().ContainSingle().Which.Should().BeSameAs(command);
    }

    [Test]
    public async Task OnCommandExecuted_Is_Not_Raised_For_SubCommands()
    {
        var raised = new List<ICommand>();
        _mediator.OnCommandExecuted += (_, cmd) => raised.Add(cmd);

        // ChangeAgeAndNameCommand dispatches two sub-commands internally
        var command = new ChangeAgeAndNameCommand(99, "Bob");
        await _mediator.SendAsync(command);

        // Only the top-level parent command should appear
        raised.Should().ContainSingle().Which.Should().BeSameAs(command);
    }

    [Test]
    public async Task SubCommands_Are_Not_Added_To_History()
    {
        // ChangeAgeAndNameCommand dispatches two sub-commands internally
        await _mediator.SendAsync(new ChangeAgeAndNameCommand(99, "Bob"));

        // Only the top-level command should be in history, not the two sub-commands
        _mediator.HistoryLength.Should().Be(1);
    }

    [Test]
    public async Task OnCommandExecuted_Is_Not_Raised_When_Command_Fails()
    {
        var raised = new List<ICommand>();
        _mediator.OnCommandExecuted += (_, cmd) => raised.Add(cmd);

        // CancelableCommand(true) returns Canceled — not Success
        await _mediator.SendAsync(new CancelableCommand(true));

        raised.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────
    //  OnCommandUndone
    // ──────────────────────────────────────────────

    [Test]
    public async Task OnCommandUndone_Is_Raised_After_Undo()
    {
        var command = new ChangeAgeCommand(42);
        await _mediator.SendAsync(command);

        ICommand? undoneCommand = null;
        _mediator.OnCommandUndone += (_, cmd) => undoneCommand = cmd;

        await _mediator.UndoLastCommandAsync();

        undoneCommand.Should().BeSameAs(command);
    }

    [Test]
    public async Task OnCommandUndone_Is_Not_Raised_When_History_Is_Empty()
    {
        var raised = false;
        _mediator.OnCommandUndone += (_, _) => raised = true;

        await _mediator.UndoLastCommandAsync();

        raised.Should().BeFalse();
    }

    // ──────────────────────────────────────────────
    //  OnCommandRedone
    // ──────────────────────────────────────────────

    [Test]
    public async Task OnCommandRedone_Is_Raised_After_Redo()
    {
        var command = new ChangeAgeCommand(42);
        await _mediator.SendAsync(command);
        await _mediator.UndoLastCommandAsync();

        ICommand? redoneCommand = null;
        _mediator.OnCommandRedone += (_, cmd) => redoneCommand = cmd;

        await _mediator.RedoLastUndoneCommandAsync();

        redoneCommand.Should().BeSameAs(command);
    }

    [Test]
    public async Task OnCommandRedone_Is_Not_Raised_When_RedoHistory_Is_Empty()
    {
        var raised = false;
        _mediator.OnCommandRedone += (_, _) => raised = true;

        await _mediator.RedoLastUndoneCommandAsync();

        raised.Should().BeFalse();
    }
}
