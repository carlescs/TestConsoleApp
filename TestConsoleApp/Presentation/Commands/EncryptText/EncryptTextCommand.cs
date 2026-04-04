using Spectre.Console;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Application.Services;

namespace TestConsoleApp.Presentation.Commands.EncryptText;

/// <summary>
/// A menu command in the <c>Utilities</c> submenu that encrypts or decrypts text using a
/// selectable algorithm with a PBKDF2-derived key. Cryptographic operations are delegated
/// to <see cref="IEncryptionService"/>.
/// </summary>
[SubMenu("Utilities")]
[Hotkey('E', ConsoleModifiers.Control)]
[CommandDescription("Encrypts or decrypts text using AES-128-GCM, AES-256-CBC, AES-256-CCM, AES-256-GCM, ChaCha20-Poly1305, or TripleDES-CBC.")]
public sealed class EncryptTextCommand(
    IAnsiConsole? console = null,
    EncryptTextSettings? cliSettings = null,
    IEncryptionService? encryptionService = null) : IMenuCommand, ICliParameterised
{
    private const string EncryptChoice = "Encrypt  →  text to ciphertext";
    private const string DecryptChoice = "Decrypt  →  ciphertext to text";

    private readonly IAnsiConsole _console              = console ?? AnsiConsole.Console;
    private readonly EncryptTextSettings? _cliSettings  = cliSettings;
    private readonly IEncryptionService _service        = encryptionService ?? new EncryptionService();

    Type ICliParameterised.SettingsType => typeof(EncryptTextSettings);
    IMenuCommand ICliParameterised.WithSettings(CommandSettings settings)
        => new EncryptTextCommand(_console, settings as EncryptTextSettings, _service);

    /// <inheritdoc/>
    public string Title => "Encrypt / Decrypt Text";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _console.Clear();

        bool decrypt = _cliSettings != null
            ? _cliSettings.Decrypt
            : _console.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select operation:")
                    .AddChoices(EncryptChoice, DecryptChoice)) == DecryptChoice;

        string textPrompt = decrypt ? "Enter ciphertext (Base64) to decrypt:" : "Enter text to encrypt:";
        string text = _cliSettings?.Text ?? _console.Prompt(new TextPrompt<string>(textPrompt));

        string key = _cliSettings?.Key ?? _console.Prompt(
            new TextPrompt<string>("Enter passphrase:")
                .Secret());

        if (decrypt)
        {
            string algorithmName = _service.DetectAlgorithmName(text);
            string decrypted     = _service.Decrypt(text, key);
            _console.MarkupLine($"[bold]Algorithm:[/] [yellow]{algorithmName}[/]");
            _console.MarkupLine($"[bold]Operation:[/] [yellow]Decrypt[/]");
            _console.MarkupLine($"[bold]Result:[/]    [green]{Markup.Escape(decrypted)}[/]");
        }
        else
        {
            string algorithm = _cliSettings != null
                ? (_cliSettings.Algorithm?.ToLowerInvariant() ?? "aes-256-cbc")
                : _console.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select algorithm:")
                        .AddChoices(_service.AlgorithmChoices));

            string encrypted     = _service.Encrypt(text, key, algorithm);
            string algorithmName = _service.DetectAlgorithmName(encrypted);
            _console.MarkupLine($"[bold]Algorithm:[/] [yellow]{algorithmName}[/]");
            _console.MarkupLine($"[bold]Operation:[/] [yellow]Encrypt[/]");
            _console.MarkupLine($"[bold]Result:[/]    [green]{Markup.Escape(encrypted)}[/]");
        }

        return Task.CompletedTask;
    }
}
