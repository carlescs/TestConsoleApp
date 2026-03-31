using NSubstitute;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Menus;

namespace TestConsoleApp.Tests.Presentation.Menus;

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
    public async Task RunAsync_WhenMenuReturnsNull_ExitsImmediately()
    {
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns((IMenuCommand?)null);
        var menu = new MainMenu([], interaction);

        await menu.RunAsync();

        interaction.Received(1).Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>());
    }

    [Fact]
    public async Task RunAsync_WhenCommandSelected_ExecutesCommand()
    {
        var command = Substitute.For<IMenuCommand>();
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns(command, (IMenuCommand?)null);
        var console = new Spectre.Console.Testing.TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        var menu = new MainMenu([command], interaction, console);

        await menu.RunAsync();

        await command.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_WhenCommandSelectedMultipleTimes_ExecutesEachTime()
    {
        var command = Substitute.For<IMenuCommand>();
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns(command, command, command, null);
        var console = new Spectre.Console.Testing.TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter);
        var menu = new MainMenu([command], interaction, console);

        await menu.RunAsync();

        await command.Received(3).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_ShowIsCalledWithMainMenuHeader()
    {
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns((IMenuCommand?)null);
        var menu = new MainMenu([], interaction);

        await menu.RunAsync();

        interaction.Received().Show(Arg.Is<string>(h => h == "Main Menu"), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>());
    }

    [Fact]
    public async Task RunAsync_ShowIsCalledWithExitLabel()
    {
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns((IMenuCommand?)null);
        var menu = new MainMenu([], interaction);

        await menu.RunAsync();

        interaction.Received().Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Is<string>(l => l == "Exit"), Arg.Any<Action?>());
    }

    [Fact]
    public async Task RunAsync_WhenCommandCancelsToken_StopsLoop()
    {
        using var cts = new CancellationTokenSource();
        var command = Substitute.For<IMenuCommand>();
        command.ExecuteAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0))
            .AndDoes(_ => cts.Cancel());
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns(command, command);
        var console = new Spectre.Console.Testing.TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        var menu = new MainMenu([command], interaction, console);

        await menu.RunAsync(cts.Token);

        await command.Received(1).ExecuteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_PassesCancellationTokenToCommand()
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
        var console = new Spectre.Console.Testing.TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        var menu = new MainMenu([command], interaction, console);

        await menu.RunAsync(cts.Token);

        Assert.Equal(cts.Token, capturedToken);
    }

    [Fact]
    public async Task RunAsync_AfterExecutingCommand_ShowsPressAnyKeyMessage()
    {
        var command = Substitute.For<IMenuCommand>();
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns(command, (IMenuCommand?)null);
        var console = new Spectre.Console.Testing.TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        var menu = new MainMenu([command], interaction, console);

        await menu.RunAsync();

        Assert.Contains("Press any key to continue", console.Output);
    }

    [Fact]
    public async Task RunAsync_BannerDoesNotThrow()
    {
        var interaction = Substitute.For<IMenuInteraction>();
        interaction.Show(Arg.Any<string>(), Arg.Any<IReadOnlyList<IMenuCommand>>(), Arg.Any<string>(), Arg.Any<Action?>())
            .Returns((IMenuCommand?)null)
            .AndDoes(ci => ci.Arg<Action?>()?.Invoke());
        var menu = new MainMenu([], interaction);

        var exception = await Record.ExceptionAsync(() => menu.RunAsync());

        Assert.Null(exception);
    }
}