using System.Security.Cryptography;
using System.Text;
using Spectre.Console;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands.HashText;

/// <summary>
/// A menu command in the <c>Utilities</c> submenu that hashes an input string using
/// a selectable algorithm (MD5, SHA-256, or SHA-512).
/// </summary>
[SubMenu("Utilities")]
[Hotkey('T', ConsoleModifiers.Control)]
[CommandDescription("Hashes a text string using MD5, SHA-256, or SHA-512.")]
public sealed class HashTextCommand(IAnsiConsole? console = null, HashTextSettings? cliSettings = null) : IMenuCommand, ICliParameterised
{
    private static readonly string[] Algorithms = ["sha256", "md5", "sha512"];

    private readonly IAnsiConsole _console = console ?? AnsiConsole.Console;
    private readonly HashTextSettings? _cliSettings = cliSettings;

    Type ICliParameterised.SettingsType => typeof(HashTextSettings);
    IMenuCommand ICliParameterised.WithSettings(CommandSettings settings)
        => new HashTextCommand(_console, settings as HashTextSettings);

    /// <inheritdoc/>
    public string Title => "Hash Text";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _console.Clear();

        string text = _cliSettings?.Text ?? _console.Prompt(
            new TextPrompt<string>("Enter text to hash:"));

        string algorithm = _cliSettings?.Algorithm?.ToLowerInvariant() ?? _console.Prompt(
            new SelectionPrompt<string>()
                .Title("Select hash algorithm:")
                .AddChoices(Algorithms));

        bool uppercase = _cliSettings?.Uppercase ?? false;

        string hash = ComputeHash(text, algorithm);
        if (uppercase) hash = hash.ToUpperInvariant();

        _console.MarkupLine($"[bold]Algorithm:[/] [yellow]{algorithm.ToUpperInvariant()}[/]");
        _console.MarkupLine($"[bold]Hash:[/]      [green]{hash}[/]");

        return Task.CompletedTask;
    }

    internal static string ComputeHash(string text, string algorithm)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        byte[] hashBytes = algorithm.ToLowerInvariant() switch
        {
            "md5"    => MD5.HashData(bytes),
            "sha512" => SHA512.HashData(bytes),
            _        => SHA256.HashData(bytes),
        };
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
