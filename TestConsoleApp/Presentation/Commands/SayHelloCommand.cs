using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

/// <summary>
/// A root-level menu command that prompts the user for their name and prints a personalised greeting.
/// </summary>
[Hotkey('H')]
public sealed class SayHelloCommand : IMenuCommand
{
    /// <inheritdoc/>
    public string Title => "Say Hello";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.Clear();

        var name = AnsiConsole.Prompt(
            new TextPrompt<string>("What is your name?")
                .AllowEmpty());

        var greeting = string.IsNullOrWhiteSpace(name) ? "Hello world!" : $"Hello {name}!";

        AnsiConsole.MarkupLine($"\n[bold green]{greeting}[/]\n");
        AnsiConsole.MarkupLine("[dim]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);

        return Task.CompletedTask;
    }
}