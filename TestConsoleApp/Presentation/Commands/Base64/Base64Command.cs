using System.Text;
using Spectre.Console;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands.Base64;

/// <summary>
/// A menu command in the <c>Utilities</c> submenu that encodes text to Base64
/// or decodes a Base64 string back to plain text.
/// </summary>
[SubMenu("Utilities")]
[Hotkey('B', ConsoleModifiers.Control)]
[CommandDescription("Encodes text to Base64 or decodes a Base64 string back to plain text.")]
public sealed class Base64Command(IAnsiConsole? console = null, Base64Settings? cliSettings = null) : IMenuCommand, ICliParameterised
{
    private const string EncodeChoice = "Encode  →  text to Base64";
    private const string DecodeChoice = "Decode  →  Base64 to text";

    private readonly IAnsiConsole _console = console ?? AnsiConsole.Console;
    private readonly Base64Settings? _cliSettings = cliSettings;

    Type ICliParameterised.SettingsType => typeof(Base64Settings);
    IMenuCommand ICliParameterised.WithSettings(CommandSettings settings)
        => new Base64Command(_console, settings as Base64Settings);

    /// <inheritdoc/>
    public string Title => "Base64 Converter";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _console.Clear();

        bool decode = _cliSettings != null
            ? _cliSettings.Decode
            : _console.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select operation:")
                    .AddChoices(EncodeChoice, DecodeChoice)) == DecodeChoice;

        string prompt = decode ? "Enter Base64 string to decode:" : "Enter text to encode:";
        string input = _cliSettings?.Text ?? _console.Prompt(new TextPrompt<string>(prompt));

        if (decode)
        {
            string decoded = Decode(input);
            _console.MarkupLine($"[bold]Operation:[/] [yellow]Decode[/]");
            _console.MarkupLine($"[bold]Result:[/]    [green]{Markup.Escape(decoded)}[/]");
        }
        else
        {
            string encoded = Encode(input);
            _console.MarkupLine($"[bold]Operation:[/] [yellow]Encode[/]");
            _console.MarkupLine($"[bold]Result:[/]    [green]{Markup.Escape(encoded)}[/]");
        }

        return Task.CompletedTask;
    }

    internal static string Encode(string text)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    internal static string Decode(string base64)
        => Encoding.UTF8.GetString(Convert.FromBase64String(base64));
}
