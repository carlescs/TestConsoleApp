using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Menus;

public sealed class MainMenu(IEnumerable<IMenuCommand> commands)
{
    private const string ExitOption = "Exit";
    private readonly IReadOnlyList<IMenuCommand> _commands = commands.ToList();

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("TestConsoleApp").Centered().Color(Color.Blue));
            AnsiConsole.Write(new Rule().RuleStyle("blue dim"));

            var choices = _commands
                .Select(c => c.Title)
                .Append(ExitOption)
                .ToList();

            var selection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Main Menu[/]")
                    .PageSize(10)
                    .AddChoices(choices));

            if (selection == ExitOption)
                break;

            var command = _commands.First(c => c.Title == selection);
            await command.ExecuteAsync(cancellationToken);
        }
    }
}
