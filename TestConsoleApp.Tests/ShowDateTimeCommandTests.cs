using System.Reflection;
using Spectre.Console.Testing;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Commands;

namespace TestConsoleApp.Tests;

public sealed class ShowDateTimeCommandTests
{
    private readonly ShowDateTimeCommand _sut = new();

    [Fact]
    public void Title_ReturnsShowDateAndTime()
    {
        Assert.Equal("Show Date & Time", _sut.Title);
    }

    [Fact]
    public void ImplementsIMenuCommand()
    {
        Assert.IsAssignableFrom<IMenuCommand>(_sut);
    }

    [Fact]
    public void HasSubMenuAttribute_InUtilitiesPath()
    {
        var attr = typeof(ShowDateTimeCommand).GetCustomAttribute<SubMenuAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(["Utilities"], attr!.Path);
    }

    [Fact]
    public void HasHotkeyAttribute_WithKeyD()
    {
        var attr = typeof(ShowDateTimeCommand).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal('D', attr!.Key);
    }

    [Fact]
    public void HasHotkeyAttribute_WithNoModifiers()
    {
        var attr = typeof(ShowDateTimeCommand).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(default, attr!.Modifiers);
    }

    [Fact]
    public async Task ExecuteAsync_OutputContainsDateTimeLabel()
    {
        var console = new TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        var command = new ShowDateTimeCommand(console);

        await command.ExecuteAsync();

        Assert.Contains("Current date and time:", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_OutputContainsCurrentYear()
    {
        var console = new TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        var command = new ShowDateTimeCommand(console);

        await command.ExecuteAsync();

        Assert.Contains(DateTime.Now.Year.ToString(), console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_PrintsPressAnyKeyMessage()
    {
        var console = new TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        var command = new ShowDateTimeCommand(console);

        await command.ExecuteAsync();

        Assert.Contains("Press any key to continue", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotThrow()
    {
        var console = new TestConsole();
        console.Input.PushKey(ConsoleKey.Enter);
        var command = new ShowDateTimeCommand(console);

        var exception = await Record.ExceptionAsync(() => command.ExecuteAsync());

        Assert.Null(exception);
    }
}
