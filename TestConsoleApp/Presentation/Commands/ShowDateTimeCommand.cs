using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

[SubMenu("Utilities")]
public sealed class ShowDateTimeCommand : IMenuCommand
{
    public string Title => "Show Date & Time";

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold]Current date and time:[/] [green]{DateTime.Now:F}[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);

        return Task.CompletedTask;
    }
}
