using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

/// <summary>
/// A menu command in the <c>Utilities</c> submenu that displays the current date and time.
/// </summary>
[SubMenu("Utilities")]
[Hotkey('D')]
public sealed class ShowDateTimeCommand(IAnsiConsole? console = null) : IMenuCommand
{
    private readonly IAnsiConsole _console = console ?? AnsiConsole.Console;

    /// <inheritdoc/>
    public string Title => "Show Date & Time";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _console.Clear();
        _console.MarkupLine($"[bold]Current date and time:[/] [green]{DateTime.Now:F}[/]");
        _console.MarkupLine("\n[dim]Press any key to continue...[/]");
        _console.Input.ReadKey(intercept: true);

        return Task.CompletedTask;
    }
}
