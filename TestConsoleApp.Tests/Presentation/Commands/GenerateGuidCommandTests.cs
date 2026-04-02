using System.Reflection;
using Spectre.Console.Testing;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Commands;
using TestConsoleApp.Presentation.Commands.GenerateGuid;

namespace TestConsoleApp.Tests.Presentation.Commands;

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
        var command = new GenerateGuidCommand(console);

        await command.ExecuteAsync();

        Assert.Contains("New GUID:", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_OutputContainsValidGuid()
    {
        var console = new TestConsole();
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
        var console2 = new TestConsole();

        await new GenerateGuidCommand(console1).ExecuteAsync();
        await new GenerateGuidCommand(console2).ExecuteAsync();

        Assert.NotEqual(console1.Output, console2.Output);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotThrow()
    {
        var console = new TestConsole();
        var command = new GenerateGuidCommand(console);

        var exception = await Record.ExceptionAsync(() => command.ExecuteAsync());

        Assert.Null(exception);
    }

    [Fact]
    public void HasCommandDescriptionAttribute()
    {
        var attr = typeof(GenerateGuidCommand).GetCustomAttribute<CommandDescriptionAttribute>();

        Assert.NotNull(attr);
        Assert.False(string.IsNullOrWhiteSpace(attr!.Description));
    }

    [Fact]
    public async Task ExecuteAsync_WithCliCount_OutputsMultipleGuids()
    {
        var console = new TestConsole();
        var command = new GenerateGuidCommand(console, new GenerateGuidSettings { Count = 3 });

        await command.ExecuteAsync();

        Assert.Equal(3, console.Output.Split("New GUID:", StringSplitOptions.RemoveEmptyEntries).Length - 1);
    }

    [Fact]
    public async Task ExecuteAsync_WithCliUppercase_OutputsUpperCaseGuids()
    {
        var console = new TestConsole();
        var command = new GenerateGuidCommand(console, new GenerateGuidSettings { Uppercase = true });

        await command.ExecuteAsync();

        string guid = console.Output
            .Split("New GUID:", StringSplitOptions.RemoveEmptyEntries)[1]
            .Trim();
        Assert.Equal(guid, guid.ToUpperInvariant());
    }

    [Fact]
    public void ImplementsICliParameterised()
    {
        Assert.IsAssignableFrom<ICliParameterised>(new GenerateGuidCommand());
    }

    [Fact]
    public void CliParameterised_SettingsType_IsGenerateGuidSettings()
    {
        ICliParameterised sut = new GenerateGuidCommand();

        Assert.Equal(typeof(GenerateGuidSettings), sut.SettingsType);
    }

    [Fact]
    public async Task CliParameterised_WithSettings_AppliesCountToOutput()
    {
        var console = new TestConsole();
        ICliParameterised sut = new GenerateGuidCommand(console);

        var configured = sut.WithSettings(new GenerateGuidSettings { Count = 2 });
        await configured.ExecuteAsync();

        Assert.Equal(2, console.Output.Split("New GUID:", StringSplitOptions.RemoveEmptyEntries).Length - 1);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultCount_OutputsOneGuid()
    {
        var console = new TestConsole();
        var command = new GenerateGuidCommand(console);

        await command.ExecuteAsync();

        Assert.Equal(1, console.Output.Split("New GUID:", StringSplitOptions.RemoveEmptyEntries).Length - 1);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultUppercase_OutputsLowercaseGuid()
    {
        var console = new TestConsole();
        var command = new GenerateGuidCommand(console);

        await command.ExecuteAsync();

        string guid = console.Output
            .Split("New GUID:", StringSplitOptions.RemoveEmptyEntries)[1]
            .Trim();
        Assert.Equal(guid, guid.ToLowerInvariant());
    }
}
