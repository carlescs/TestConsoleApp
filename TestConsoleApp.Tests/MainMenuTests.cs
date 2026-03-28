using NSubstitute;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Menus;

namespace TestConsoleApp.Tests;

public sealed class MainMenuTests
{
    [Fact]
    public async Task RunAsync_WhenTokenAlreadyCancelled_ReturnsImmediately()
    {
        var command = Substitute.For<IMenuCommand>();
        command.Title.Returns("Test Command");
        var menu = new MainMenu([command]);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await menu.RunAsync(cts.Token);

        await command.DidNotReceive().ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenTokenAlreadyCancelled_DoesNotThrow()
    {
        var menu = new MainMenu([]);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var exception = await Record.ExceptionAsync(() => menu.RunAsync(cts.Token));

        Assert.Null(exception);
    }

    [Fact]
    public async Task RunAsync_WithMultipleCommands_WhenTokenAlreadyCancelled_DoesNotExecuteAny()
    {
        var cmd1 = Substitute.For<IMenuCommand>();
        var cmd2 = Substitute.For<IMenuCommand>();
        cmd1.Title.Returns("First");
        cmd2.Title.Returns("Second");
        var menu = new MainMenu([cmd1, cmd2]);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await menu.RunAsync(cts.Token);

        await cmd1.DidNotReceive().ExecuteAsync(Arg.Any<CancellationToken>());
        await cmd2.DidNotReceive().ExecuteAsync(Arg.Any<CancellationToken>());
    }
}
