using System.Reflection;
using Spectre.Console.Testing;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Commands;

namespace TestConsoleApp.Tests;

public sealed class SayHelloCommandTests
{
    private readonly SayHelloCommand _sut = new();

    [Fact]
    public void Title_ReturnsSayHello()
    {
        Assert.Equal("Say Hello", _sut.Title);
    }

    [Fact]
    public void ImplementsIMenuCommand()
    {
        Assert.IsAssignableFrom<IMenuCommand>(_sut);
    }

    [Fact]
    public void HasHotkeyAttribute_WithKeyH()
    {
        var attr = typeof(SayHelloCommand).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal('H', attr.Key);
    }

    [Fact]
    public void HasHotkeyAttribute_WithControlModifier()
    {
        var attr = typeof(SayHelloCommand).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(ConsoleModifiers.Control, attr.Modifiers);
    }

    [Fact]
    public void DoesNotHaveSubMenuAttribute()
    {
        var attr = typeof(SayHelloCommand).GetCustomAttribute<SubMenuAttribute>();

        Assert.Null(attr);
    }

    [Fact]
    public async Task ExecuteAsync_WithName_PrintsPersonalisedGreeting()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("Alice");
        console.Input.PushKey(ConsoleKey.Enter); // "press any key to continue"
        var command = new SayHelloCommand(console);

        await command.ExecuteAsync();

        Assert.Contains("Hello Alice!", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyInput_PrintsHelloWorld()
    {
        var console = new TestConsole();
        console.Input.PushKey(ConsoleKey.Enter); // empty TextPrompt input
        console.Input.PushKey(ConsoleKey.Enter); // "press any key to continue"
        var command = new SayHelloCommand(console);

        await command.ExecuteAsync();

        Assert.Contains("Hello world!", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithWhitespaceInput_PrintsHelloWorld()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("   ");
        console.Input.PushKey(ConsoleKey.Enter); // "press any key to continue"
        var command = new SayHelloCommand(console);

        await command.ExecuteAsync();

        Assert.Contains("Hello world!", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_PrintsPressAnyKeyMessage()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("Bob");
        console.Input.PushKey(ConsoleKey.Enter);
        var command = new SayHelloCommand(console);

        await command.ExecuteAsync();

        Assert.Contains("Press any key to continue", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotThrow()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("Test");
        console.Input.PushKey(ConsoleKey.Enter);
        var command = new SayHelloCommand(console);

        var exception = await Record.ExceptionAsync(() => command.ExecuteAsync());

        Assert.Null(exception);
    }
}
