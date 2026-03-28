using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

/// <summary>
/// A menu command in the <c>Utilities</c> submenu that generates and displays a new <see cref="Guid"/>.
/// </summary>
[SubMenu("Utilities")]
[Hotkey('G')]
public sealed class GenerateGuidCommand : IMenuCommand
{
    /// <inheritdoc/>
    public string Title => "Generate GUID";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($"[bold]New GUID:[/] [green]{Guid.NewGuid()}[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);

        return Task.CompletedTask;
    }
}
