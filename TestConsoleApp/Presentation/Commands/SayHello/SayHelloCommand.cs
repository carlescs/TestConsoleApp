using Spectre.Console;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

/// <summary>
/// A root-level menu command that prompts the user for their name and prints a personalised greeting.
/// </summary>
[Hotkey('H', ConsoleModifiers.Control)]
[CommandDescription("Asks for your name and greets you — or greets the world when no name is given.")]
public sealed class SayHelloCommand(IAnsiConsole? console = null, SayHelloSettings? cliSettings = null) : IMenuCommand, ICliParameterised
{
    private readonly IAnsiConsole _console = console ?? AnsiConsole.Console;
    private readonly SayHelloSettings? _cliSettings = cliSettings;

    Type ICliParameterised.SettingsType => typeof(SayHelloSettings);
    IMenuCommand ICliParameterised.WithSettings(CommandSettings settings)
        => new SayHelloCommand(_console, settings as SayHelloSettings);

    /// <inheritdoc/>
    public string Title => "Say Hello";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _console.Clear();

        string name = _cliSettings?.Name is { } n
            ? n
            : _console.Prompt(
                new TextPrompt<string>("What is your name?")
                    .AllowEmpty());

        string greeting = string.IsNullOrWhiteSpace(name) ? "Hello world!" : $"Hello {name}!";

        _console.MarkupLine($"\n[bold green]{greeting}[/]\n");

        return Task.CompletedTask;
    }
}
