using System.Reflection;
using Spectre.Console.Testing;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Commands;
using TestConsoleApp.Presentation.Commands.Base64;

namespace TestConsoleApp.Tests.Presentation.Commands;

public sealed class Base64CommandTests
{
    private readonly Base64Command _sut = new();

    [Fact]
    public void Title_ReturnsBase64Converter()
    {
        Assert.Equal("Base64 Converter", _sut.Title);
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
        var attr = typeof(Base64Command).GetCustomAttribute<SubMenuAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(["Utilities"], attr!.Path);
    }

    [Fact]
    public void HasHotkeyAttribute_WithKeyB()
    {
        var attr = typeof(Base64Command).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal('B', attr!.Key);
    }

    [Fact]
    public void HasHotkeyAttribute_WithControlModifier()
    {
        var attr = typeof(Base64Command).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(ConsoleModifiers.Control, attr!.Modifiers);
    }

    [Fact]
    public void HasCommandDescriptionAttribute()
    {
        var attr = typeof(Base64Command).GetCustomAttribute<CommandDescriptionAttribute>();

        Assert.NotNull(attr);
        Assert.False(string.IsNullOrWhiteSpace(attr!.Description));
    }

    [Fact]
    public void CliParameterised_SettingsType_IsBase64Settings()
    {
        ICliParameterised sut = new Base64Command();

        Assert.Equal(typeof(Base64Settings), sut.SettingsType);
    }

    [Theory]
    [InlineData("hello", "aGVsbG8=")]
    [InlineData("Hello, World!", "SGVsbG8sIFdvcmxkIQ==")]
    [InlineData("", "")]
    public void Encode_ReturnsExpectedBase64(string input, string expected)
    {
        Assert.Equal(expected, Base64Command.Encode(input));
    }

    [Theory]
    [InlineData("aGVsbG8=", "hello")]
    [InlineData("SGVsbG8sIFdvcmxkIQ==", "Hello, World!")]
    [InlineData("", "")]
    public void Decode_ReturnsExpectedText(string input, string expected)
    {
        Assert.Equal(expected, Base64Command.Decode(input));
    }

    [Fact]
    public void EncodeThenDecode_RoundTrips()
    {
        const string original = "GitHub Copilot";

        Assert.Equal(original, Base64Command.Decode(Base64Command.Encode(original)));
    }

    [Fact]
    public async Task ExecuteAsync_Encode_OutputContainsEncodedResult()
    {
        var console = new TestConsole();
        var command = new Base64Command(console, new Base64Settings { Text = "hello", Decode = false });

        await command.ExecuteAsync();

        Assert.Contains("aGVsbG8=", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_Encode_OutputContainsEncodeLabel()
    {
        var console = new TestConsole();
        var command = new Base64Command(console, new Base64Settings { Text = "hello", Decode = false });

        await command.ExecuteAsync();

        Assert.Contains("Encode", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_Decode_OutputContainsDecodedText()
    {
        var console = new TestConsole();
        var command = new Base64Command(console, new Base64Settings { Text = "aGVsbG8=", Decode = true });

        await command.ExecuteAsync();

        Assert.Contains("hello", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_Decode_OutputContainsDecodeLabel()
    {
        var console = new TestConsole();
        var command = new Base64Command(console, new Base64Settings { Text = "aGVsbG8=", Decode = true });

        await command.ExecuteAsync();

        Assert.Contains("Decode", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotThrow()
    {
        var console = new TestConsole();
        var command = new Base64Command(console, new Base64Settings { Text = "test", Decode = false });

        var exception = await Record.ExceptionAsync(() => command.ExecuteAsync());

        Assert.Null(exception);
    }

    [Fact]
    public async Task CliParameterised_WithSettings_AppliesDecodeToOutput()
    {
        var console = new TestConsole();
        ICliParameterised sut = new Base64Command(console);

        var configured = sut.WithSettings(new Base64Settings { Text = "aGVsbG8=", Decode = true });
        await configured.ExecuteAsync();

        Assert.Contains("hello", console.Output);
    }
}
