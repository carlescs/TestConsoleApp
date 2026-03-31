using NSubstitute;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Menus;

namespace TestConsoleApp.Tests.Presentation.Menus;

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

    [Fact]
    public void ChildCommands_IsEmpty_WhenNoCommandsProvided()
    {
        var subMenu = new SubMenuCommand("Test", []);

        Assert.Empty(subMenu.ChildCommands);
    }

    [Fact]
    public void ChildCommands_ReturnsCommandsPassedToConstructor()
    {
        var cmd1 = Substitute.For<IMenuCommand>();
        var cmd2 = Substitute.For<IMenuCommand>();
        var subMenu = new SubMenuCommand("Test", [cmd1, cmd2]);

        Assert.Equal(2, subMenu.ChildCommands.Count);
        Assert.Contains(cmd1, subMenu.ChildCommands);
        Assert.Contains(cmd2, subMenu.ChildCommands);
    }

    [Fact]
    public void ChildCommands_PreservesInsertionOrder()
    {
        var cmd1 = Substitute.For<IMenuCommand>();
        var cmd2 = Substitute.For<IMenuCommand>();
        var cmd3 = Substitute.For<IMenuCommand>();
        var subMenu = new SubMenuCommand("Test", [cmd1, cmd2, cmd3]);

        Assert.Equal(cmd1, subMenu.ChildCommands[0]);
        Assert.Equal(cmd2, subMenu.ChildCommands[1]);
        Assert.Equal(cmd3, subMenu.ChildCommands[2]);
    }

    [Fact]
    public void ChildCommands_IsReadOnly()
    {
        var subMenu = new SubMenuCommand("Test", []);

        Assert.IsAssignableFrom<IReadOnlyList<IMenuCommand>>(subMenu.ChildCommands);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMenuReturnsNull_ExitsImmediately()
    {
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns((IMenuCommand?)null);
        var subMenu = new SubMenuCommand("Test", [], interaction);

        await subMenu.ExecuteAsync();

        interaction.Received(1).Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCommandSelected_ExecutesCommand()
    {
        IMenuCommand? command = Substitute.For<IMenuCommand>();
        IMenuInteraction? interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns(command, (IMenuCommand?)null);
        var subMenu = new SubMenuCommand("Test", [command], interaction);

        await subMenu.ExecuteAsync();

        await command.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCommandSelectedMultipleTimes_ExecutesEachTime()
    {
        var command = Substitute.For<IMenuCommand>();
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns(command, command, command, null);
        var subMenu = new SubMenuCommand("Test", [command], interaction);

        await subMenu.ExecuteAsync();

        await command.Received(3).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ShowIsCalledWithSubMenuTitle()
    {
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns((IMenuCommand?)null);
        var subMenu = new SubMenuCommand("My Submenu", [], interaction);

        await subMenu.ExecuteAsync();

        interaction.Received().Show(Arg.Is<string>(h => h == "My Submenu"), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>());
    }

    [Fact]
    public async Task ExecuteAsync_ShowIsCalledWithBackLabel()
    {
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns((IMenuCommand?)null);
        var subMenu = new SubMenuCommand("Test", [], interaction);

        await subMenu.ExecuteAsync();

        interaction.Received().Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Is<string>(l => l == "Back"), Arg.Any<Action?>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCommandCancelsToken_StopsLoop()
    {
        using var cts = new CancellationTokenSource();
        var command = Substitute.For<IMenuCommand>();
        command.ExecuteAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0))
            .AndDoes(_ => cts!.Cancel());
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns(command, command);
        var subMenu = new SubMenuCommand("Test", [command], interaction);

        await subMenu.ExecuteAsync(cts.Token);

        await command.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesCancellationTokenToCommand()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = CancellationToken.None;
        var command = Substitute.For<IMenuCommand>();
        command.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(0))
            .AndDoes(ci => capturedToken = ci.Arg<CancellationToken>());
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns(command, (IMenuCommand?)null);
        var subMenu = new SubMenuCommand("Test", [command], interaction);

        await subMenu.ExecuteAsync(cts.Token);

        Assert.Equal(cts.Token, capturedToken);
    }
}
