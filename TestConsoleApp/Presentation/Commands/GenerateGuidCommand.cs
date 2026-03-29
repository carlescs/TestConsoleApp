using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

/// <summary>
/// A menu command in the <c>Utilities</c> submenu that generates and displays a new <see cref="Guid"/>.
/// </summary>
[SubMenu("Utilities")]
[Hotkey('G')]
public sealed class GenerateGuidCommand(IAnsiConsole? console = null) : IMenuCommand
{
    private readonly IAnsiConsole _console = console ?? AnsiConsole.Console;

    /// <inheritdoc/>
    public string Title => "Generate GUID";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _console.Clear();
        _console.MarkupLine($"[bold]New GUID:[/] [green]{Guid.NewGuid()}[/]");
        _console.MarkupLine("\n[dim]Press any key to continue...[/]");
        _console.Input.ReadKey(intercept: true);

        return Task.CompletedTask;
    }
}
