using NSubstitute;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Menus;

namespace TestConsoleApp.Tests;

public sealed class SubMenuCommandTests
{
    [Fact]
    public void Title_ReturnsTitlePassedToConstructor()
    {
        var subMenu = new SubMenuCommand("My Menu", []);

        Assert.Equal("My Menu", subMenu.Title);
    }

    [Fact]
    public void ImplementsIMenuCommand()
    {
        var subMenu = new SubMenuCommand("Test", []);

        Assert.IsAssignableFrom<IMenuCommand>(subMenu);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTokenAlreadyCancelled_ReturnsImmediately()
    {
        var command = Substitute.For<IMenuCommand>();
        var subMenu = new SubMenuCommand("Test", [command]);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await subMenu.ExecuteAsync(cts.Token);

        await command.DidNotReceive().ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTokenAlreadyCancelled_DoesNotThrow()
    {
        var subMenu = new SubMenuCommand("Test", []);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var exception = await Record.ExceptionAsync(() => subMenu.ExecuteAsync(cts.Token));

        Assert.Null(exception);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTokenAlreadyCancelled_DoesNotInteractWithChildCommands()
    {
        var cmd1 = Substitute.For<IMenuCommand>();
        var cmd2 = Substitute.For<IMenuCommand>();
        cmd1.Title.Returns("A");
        cmd2.Title.Returns("B");
        var subMenu = new SubMenuCommand("Test", [cmd1, cmd2]);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await subMenu.ExecuteAsync(cts.Token);

        _ = cmd1.DidNotReceive().Title;
        await cmd1.DidNotReceive().ExecuteAsync(Arg.Any<CancellationToken>());
        await cmd2.DidNotReceive().ExecuteAsync(Arg.Any<CancellationToken>());
    }
}
