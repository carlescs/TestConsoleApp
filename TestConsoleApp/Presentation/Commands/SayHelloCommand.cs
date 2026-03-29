using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

/// <summary>
/// A root-level menu command that prompts the user for their name and prints a personalised greeting.
/// </summary>
[Hotkey('H')]
public sealed class SayHelloCommand(IAnsiConsole? console = null) : IMenuCommand
{
    private readonly IAnsiConsole _console = console ?? AnsiConsole.Console;

    /// <inheritdoc/>
    public string Title => "Say Hello";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _console.Clear();

        var name = _console.Prompt(
            new TextPrompt<string>("What is your name?")
                .AllowEmpty());

        var greeting = string.IsNullOrWhiteSpace(name) ? "Hello world!" : $"Hello {name}!";

        _console.MarkupLine($"\n[bold green]{greeting}[/]\n");
        _console.MarkupLine("[dim]Press any key to continue...[/]");
        _console.Input.ReadKey(intercept: true);

        return Task.CompletedTask;
    }
}
