using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Menus;

/// <summary>
/// Presents the top-level interactive menu using Spectre.Console, looping until the user
/// chooses to exit or the cancellation token is cancelled.
/// </summary>
/// <param name="commands">The commands to display at the root level of the menu.</param>
public sealed class MainMenu(IEnumerable<IMenuCommand> commands)
{
    private const string ExitOption = "Exit";
    private readonly IReadOnlyList<IMenuCommand> _commands = commands.ToList();

    /// <summary>
    /// Runs the menu loop, rendering choices and dispatching to the selected command,
    /// until the user selects <c>Exit</c> or <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
