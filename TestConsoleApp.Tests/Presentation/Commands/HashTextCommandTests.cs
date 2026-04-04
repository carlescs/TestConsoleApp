using System.Reflection;
using Spectre.Console.Testing;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Commands;
using TestConsoleApp.Presentation.Commands.HashText;

namespace TestConsoleApp.Tests.Presentation.Commands;

public sealed class HashTextCommandTests
{
    private readonly HashTextCommand _sut = new();

    [Fact]
    public void Title_ReturnsHashText()
    {
        Assert.Equal("Hash Text", _sut.Title);
    }

    [Fact]
    public void ImplementsIMenuCommand()
    {
        Assert.IsAssignableFrom<IMenuCommand>(_sut);
    }

    [Fact]
    public void ImplementsICliParameterised()
    {
        Assert.IsAssignableFrom<ICliParameterised>(_sut);
    }

    [Fact]
    public void HasSubMenuAttribute_InUtilitiesPath()
    {
        var attr = typeof(HashTextCommand).GetCustomAttribute<SubMenuAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(["Utilities"], attr!.Path);
    }

    [Fact]
    public void HasHotkeyAttribute_WithKeyT()
    {
        var attr = typeof(HashTextCommand).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal('T', attr!.Key);
    }

    [Fact]
    public void HasHotkeyAttribute_WithControlModifier()
    {
        var attr = typeof(HashTextCommand).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(ConsoleModifiers.Control, attr!.Modifiers);
    }

    [Fact]
    public void HasCommandDescriptionAttribute()
    {
        var attr = typeof(HashTextCommand).GetCustomAttribute<CommandDescriptionAttribute>();

        Assert.NotNull(attr);
        Assert.False(string.IsNullOrWhiteSpace(attr!.Description));
    }

    [Fact]
    public void CliParameterised_SettingsType_IsHashTextSettings()
    {
        ICliParameterised sut = new HashTextCommand();

        Assert.Equal(typeof(HashTextSettings), sut.SettingsType);
    }

    [Fact]
    public async Task ExecuteAsync_OutputContainsAlgorithmLabel()
    {
        var console = new TestConsole();
        var command = new HashTextCommand(console, new HashTextSettings { Text = "hello", Algorithm = "sha256" });

        await command.ExecuteAsync();

        Assert.Contains("Algorithm:", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_OutputContainsHashLabel()
    {
        var console = new TestConsole();
        var command = new HashTextCommand(console, new HashTextSettings { Text = "hello", Algorithm = "sha256" });

        await command.ExecuteAsync();

        Assert.Contains("Hash:", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotThrow()
    {
        var console = new TestConsole();
        var command = new HashTextCommand(console, new HashTextSettings { Text = "hello", Algorithm = "sha256" });

        var exception = await Record.ExceptionAsync(() => command.ExecuteAsync());

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("md5",    "hello", "5d41402abc4b2a76b9719d911017c592")]
    [InlineData("sha256", "hello", "2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824")]
    [InlineData("sha512", "hello", "9b71d224bd62f3785d96d46ad3ea3d73319bfbc2890caadae2dff72519673ca72323c3d99ba5c11d7c7acc6e14b8c5da0c4663475c2e5c3adef46f73bcdec043")]
    public void ComputeHash_ReturnsExpectedHash(string algorithm, string input, string expectedHash)
    {
        string actual = HashTextCommand.ComputeHash(input, algorithm);

        Assert.Equal(expectedHash, actual);
    }

    [Fact]
    public async Task ExecuteAsync_WithUppercase_OutputsUpperCaseHash()
    {
        var console = new TestConsole();
        var command = new HashTextCommand(console, new HashTextSettings { Text = "hello", Algorithm = "sha256", Uppercase = true });

        await command.ExecuteAsync();

        string expectedHash = HashTextCommand.ComputeHash("hello", "sha256").ToUpperInvariant();
        Assert.Contains(expectedHash, console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithCliSettings_SkipsPrompts()
    {
        var console = new TestConsole();
        var command = new HashTextCommand(console, new HashTextSettings { Text = "world", Algorithm = "md5" });

        // No input pushed — prompts must be skipped
        await command.ExecuteAsync();

        Assert.Contains("MD5", console.Output);
    }

    [Fact]
    public async Task CliParameterised_WithSettings_AppliesAlgorithmToOutput()
    {
        var console = new TestConsole();
        ICliParameterised sut = new HashTextCommand(console);

        var configured = sut.WithSettings(new HashTextSettings { Text = "test", Algorithm = "sha512" });
        await configured.ExecuteAsync();

        Assert.Contains("SHA512", console.Output);
    }
}
