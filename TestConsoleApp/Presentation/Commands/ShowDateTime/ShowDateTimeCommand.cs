using Spectre.Console;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands.ShowDateTime;

/// <summary>
/// A menu command in the <c>Utilities</c> submenu that displays the current date and time.
/// </summary>
[SubMenu("Utilities")]
[Hotkey('D', ConsoleModifiers.Control)]
[CommandDescription("Displays the current date and time in full format.")]
public sealed class ShowDateTimeCommand(IAnsiConsole? console = null, ShowDateTimeSettings? cliSettings = null) : IMenuCommand, ICliParameterised
{
    private readonly IAnsiConsole _console = console ?? AnsiConsole.Console;
    private readonly ShowDateTimeSettings? _cliSettings = cliSettings;

    Type ICliParameterised.SettingsType => typeof(ShowDateTimeSettings);
    IMenuCommand ICliParameterised.WithSettings(CommandSettings settings)
        => new ShowDateTimeCommand(_console, settings as ShowDateTimeSettings);

    /// <inheritdoc/>
    public string Title => "Show Date & Time";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        string format = _cliSettings?.Format ?? "F";
        _console.Clear();
        _console.MarkupLine($"[bold]Current date and time:[/] [green]{DateTime.Now.ToString(format)}[/]");

        return Task.CompletedTask;
    }
}
