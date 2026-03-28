using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Menus;

public sealed class SubMenuCommand(string title, IReadOnlyList<IMenuCommand> commands) : IMenuCommand
{
    private const string BackOption = "Back";

    public string Title => title;
    public IReadOnlyList<IMenuCommand> ChildCommands => commands;

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[bold]{Markup.Escape(title)}[/]").RuleStyle("blue dim"));

            var choices = commands
                .Select(c => c.Title)
                .Append(BackOption)
                .ToList();

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]{Markup.Escape(title)}[/]")
                    .PageSize(10)
                    .AddChoices(choices));

            if (selection == BackOption)
                break;

            var command = commands.First(c => c.Title == selection);
            await command.ExecuteAsync(cancellationToken);
        }
    }
}
