using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

[SubMenu("Utilities")]
public sealed class GenerateGuidCommand : IMenuCommand
{
    public string Title => "Generate GUID";

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold]New GUID:[/] [green]{Guid.NewGuid()}[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);

        return Task.CompletedTask;
    }
}
