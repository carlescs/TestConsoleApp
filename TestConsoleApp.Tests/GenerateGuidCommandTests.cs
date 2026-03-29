using System.Reflection;
using Spectre.Console.Testing;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Commands;

namespace TestConsoleApp.Tests;

public sealed class GenerateGuidCommandTests
{
    private readonly GenerateGuidCommand _sut = new();

    [Fact]
    public void Title_ReturnsGenerateGuid()
    {
        Assert.Equal("Generate GUID", _sut.Title);
    }

    [Fact]
    public void ImplementsIMenuCommand()
    {
        Assert.IsAssignableFrom<IMenuCommand>(_sut);
    }

    [Fact]
    public void HasSubMenuAttribute_InUtilitiesPath()
    {
        var attr = typeof(GenerateGuidCommand).GetCustomAttribute<SubMenuAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(["Utilities"], attr!.Path);
    }

    [Fact]
    public void HasHotkeyAttribute_WithKeyG()
    {
        var attr = typeof(GenerateGuidCommand).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal('G', attr!.Key);
    }

    [Fact]
    public void HasHotkeyAttribute_WithControlModifier()
    {
        var attr = typeof(GenerateGuidCommand).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(ConsoleModifiers.Control, attr!.Modifiers);
    }

    [Fact]
    public async Task ExecuteAsync_OutputContainsGuidLabel()
    {
        var console = new TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        var command = new GenerateGuidCommand(console);

        await command.ExecuteAsync();

        Assert.Contains("New GUID:", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_OutputContainsValidGuid()
    {
        var console = new TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        var command = new GenerateGuidCommand(console);

        await command.ExecuteAsync();

        Assert.Matches(
            @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
            console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_GeneratesDifferentGuidOnEachCall()
    {
        var console1 = new TestConsole();
        console1.Input.PushKey(ConsoleKey.Enter);
        var console2 = new TestConsole();
        console2.Input.PushKey(ConsoleKey.Enter);

        await new GenerateGuidCommand(console1).ExecuteAsync();
        await new GenerateGuidCommand(console2).ExecuteAsync();

        Assert.NotEqual(console1.Output, console2.Output);
    }

    [Fact]
    public async Task ExecuteAsync_PrintsPressAnyKeyMessage()
    {
        var console = new TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        var command = new GenerateGuidCommand(console);

        await command.ExecuteAsync();

        Assert.Contains("Press any key to continue", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotThrow()
    {
        var console = new TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        var command = new GenerateGuidCommand(console);

        var exception = await Record.ExceptionAsync(() => command.ExecuteAsync());

        Assert.Null(exception);
    }
}
