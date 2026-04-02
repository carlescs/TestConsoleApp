using System.Reflection;
using Spectre.Console.Testing;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Commands;
using TestConsoleApp.Presentation.Commands.SayHello;

namespace TestConsoleApp.Tests.Presentation.Commands;

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
        var command = new SayHelloCommand(console);

        await command.ExecuteAsync();

        Assert.Contains("Hello Alice!", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyInput_PrintsHelloWorld()
    {
        var console = new TestConsole();
        console.Input.PushKey(ConsoleKey.Enter); // empty TextPrompt input
        var command = new SayHelloCommand(console);

        await command.ExecuteAsync();

        Assert.Contains("Hello world!", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithWhitespaceInput_PrintsHelloWorld()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("   ");
        var command = new SayHelloCommand(console);

        await command.ExecuteAsync();

        Assert.Contains("Hello world!", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotThrow()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("Test");
        var command = new SayHelloCommand(console);

        var exception = await Record.ExceptionAsync(() => command.ExecuteAsync());

        Assert.Null(exception);
    }

    [Fact]
    public void HasCommandDescriptionAttribute()
    {
        var attr = typeof(SayHelloCommand).GetCustomAttribute<CommandDescriptionAttribute>();

        Assert.NotNull(attr);
        Assert.False(string.IsNullOrWhiteSpace(attr.Description));
    }

    [Fact]
    public async Task ExecuteAsync_WithCliName_SkipsPromptAndGreets()
    {
        var console = new TestConsole();
        var command = new SayHelloCommand(console, new SayHelloSettings { Name = "Alice" });

        await command.ExecuteAsync();

        Assert.Contains("Hello Alice!", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithCliEmptyName_PrintsHelloWorld()
    {
        var console = new TestConsole();
        var command = new SayHelloCommand(console, new SayHelloSettings { Name = "" });

        await command.ExecuteAsync();

        Assert.Contains("Hello world!", console.Output);
    }

    [Fact]
    public void ImplementsICliParameterised()
    {
        Assert.IsAssignableFrom<ICliParameterised>(new SayHelloCommand());
    }

    [Fact]
    public void CliParameterised_SettingsType_IsSayHelloSettings()
    {
        ICliParameterised sut = new SayHelloCommand();

        Assert.Equal(typeof(SayHelloSettings), sut.SettingsType);
    }

    [Fact]
    public async Task CliParameterised_WithSettings_GreetsWithProvidedName()
    {
        var console = new TestConsole();
        ICliParameterised sut = new SayHelloCommand(console);

        var configured = sut.WithSettings(new SayHelloSettings { Name = "Bob" });
        await configured.ExecuteAsync();

        Assert.Contains("Hello Bob!", console.Output);
    }
}
