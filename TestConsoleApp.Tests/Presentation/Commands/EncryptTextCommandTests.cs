using System.Reflection;
using Spectre.Console.Testing;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Application.Services;
using TestConsoleApp.Presentation.Commands;
using TestConsoleApp.Presentation.Commands.EncryptText;

namespace TestConsoleApp.Tests.Presentation.Commands;

public sealed class EncryptTextCommandTests
{
    private readonly EncryptTextCommand _sut = new();

    [Fact]
    public void Title_ReturnsEncryptDecryptText()
    {
        Assert.Equal("Encrypt / Decrypt Text", _sut.Title);
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
        var attr = typeof(EncryptTextCommand).GetCustomAttribute<SubMenuAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(["Utilities"], attr!.Path);
    }

    [Fact]
    public void HasHotkeyAttribute_WithKeyE()
    {
        var attr = typeof(EncryptTextCommand).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal('E', attr!.Key);
    }

    [Fact]
    public void HasHotkeyAttribute_WithControlModifier()
    {
        var attr = typeof(EncryptTextCommand).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(ConsoleModifiers.Control, attr!.Modifiers);
    }

    [Fact]
    public void HasCommandDescriptionAttribute()
    {
        var attr = typeof(EncryptTextCommand).GetCustomAttribute<CommandDescriptionAttribute>();

        Assert.NotNull(attr);
        Assert.False(string.IsNullOrWhiteSpace(attr!.Description));
    }

    [Fact]
    public void CliParameterised_SettingsType_IsEncryptTextSettings()
    {
        ICliParameterised sut = new EncryptTextCommand();

        Assert.Equal(typeof(EncryptTextSettings), sut.SettingsType);
    }

    [Fact]
    public async Task ExecuteAsync_Encrypt_OutputContainsEncryptLabel()
    {
        var console = new TestConsole();
        var command = new EncryptTextCommand(console, new EncryptTextSettings { Text = "hello", Key = "pass", Decrypt = false });

        await command.ExecuteAsync();

        Assert.Contains("Encrypt", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_Encrypt_OutputContainsBase64Result()
    {
        var console = new TestConsole();
        var command = new EncryptTextCommand(console, new EncryptTextSettings { Text = "hello", Key = "pass", Decrypt = false });

        await command.ExecuteAsync();

        Assert.Contains("Result:", console.Output);
        // The ciphertext is a Base64 string — verify it's present and non-empty.
        string output = console.Output;
        int resultIndex = output.IndexOf("Result:", StringComparison.Ordinal);
        Assert.True(resultIndex >= 0);
        Assert.True(output[(resultIndex + 7)..].Trim().Length > 0);
    }

    [Fact]
    public async Task ExecuteAsync_Decrypt_OutputContainsDecryptLabel()
    {
        string ciphertext = new EncryptionService().Encrypt("hello", "pass");
        var console = new TestConsole();
        var command = new EncryptTextCommand(console, new EncryptTextSettings { Text = ciphertext, Key = "pass", Decrypt = true });

        await command.ExecuteAsync();

        Assert.Contains("Decrypt", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_Decrypt_OutputContainsOriginalText()
    {
        string ciphertext = new EncryptionService().Encrypt("hello", "pass");
        var console = new TestConsole();
        var command = new EncryptTextCommand(console, new EncryptTextSettings { Text = ciphertext, Key = "pass", Decrypt = true });

        await command.ExecuteAsync();

        Assert.Contains("hello", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotThrow()
    {
        var console = new TestConsole();
        var command = new EncryptTextCommand(console, new EncryptTextSettings { Text = "test", Key = "key", Decrypt = false });

        var exception = await Record.ExceptionAsync(() => command.ExecuteAsync());

        Assert.Null(exception);
    }

    [Fact]
    public async Task CliParameterised_WithSettings_AppliesDecryptToOutput()
    {
        string ciphertext = new EncryptionService().Encrypt("round-trip", "secret");
        var console = new TestConsole();
        ICliParameterised sut = new EncryptTextCommand(console);

        var configured = sut.WithSettings(new EncryptTextSettings { Text = ciphertext, Key = "secret", Decrypt = true });
        await configured.ExecuteAsync();

        Assert.Contains("round-trip", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_Decrypt_OutputContainsAlgorithmLabel()
    {
        string ciphertext = new EncryptionService().Encrypt("hello", "pass", "aes-256-gcm");
        var console = new TestConsole();
        var command = new EncryptTextCommand(console, new EncryptTextSettings { Text = ciphertext, Key = "pass", Decrypt = true });

        await command.ExecuteAsync();

        Assert.Contains("AES-256-GCM", console.Output);
    }
}
