using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

public sealed class SayHelloCommand : IMenuCommand
{
    public string Title => "Say Hello";

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