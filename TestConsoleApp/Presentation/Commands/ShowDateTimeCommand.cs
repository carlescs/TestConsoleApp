using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

/// <summary>
/// A menu command in the <c>Utilities</c> submenu that displays the current date and time.
/// </summary>
[SubMenu("Utilities")]
public sealed class ShowDateTimeCommand : IMenuCommand
{
    /// <inheritdoc/>
    public string Title => "Show Date & Time";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold]Current date and time:[/] [green]{DateTime.Now:F}[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);

        return Task.CompletedTask;
    }
}
