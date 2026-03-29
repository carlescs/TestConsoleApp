using Spectre.Console;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

/// <summary>
/// A menu command in the <c>Utilities</c> submenu that generates and displays a new <see cref="Guid"/>.
/// </summary>
[SubMenu("Utilities")]
[Hotkey('G', ConsoleModifiers.Control)]
[CommandDescription("Generates and displays a new random GUID.")]
public sealed class GenerateGuidCommand(IAnsiConsole? console = null, GenerateGuidSettings? cliSettings = null) : IMenuCommand, ICliParameterised
{
    private readonly IAnsiConsole _console = console ?? AnsiConsole.Console;
    private readonly GenerateGuidSettings? _cliSettings = cliSettings;

    Type ICliParameterised.SettingsType => typeof(GenerateGuidSettings);
    IMenuCommand ICliParameterised.WithSettings(CommandSettings settings)
        => new GenerateGuidCommand(_console, settings as GenerateGuidSettings);

    /// <inheritdoc/>
    public string Title => "Generate GUID";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int count = _cliSettings?.Count ?? 1;
        bool uppercase = _cliSettings?.Uppercase ?? false;

        _console.Clear();

        for (int i = 0; i < count; i++)
        {
            string guid = Guid.NewGuid().ToString();
            if (uppercase) guid = guid.ToUpperInvariant();
            _console.MarkupLine($"[bold]New GUID:[/] [green]{guid}[/]");
        }

        return Task.CompletedTask;
    }
}
