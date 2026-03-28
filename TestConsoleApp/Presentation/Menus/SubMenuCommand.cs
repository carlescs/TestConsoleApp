using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Menus;

/// <summary>
/// An <see cref="IMenuCommand"/> that presents a nested submenu of child commands,
/// looping until the user selects <c>Back</c> or the cancellation token is cancelled.
/// </summary>
/// <param name="title">The display title shown as the menu heading and in parent menus.</param>
/// <param name="commands">The child commands available within this submenu.</param>
public sealed class SubMenuCommand(string title, IReadOnlyList<IMenuCommand> commands) : IMenuCommand
{
    private const string BackOption = "Back";

    /// <inheritdoc/>
    public string Title => title;

    /// <summary>Gets the child commands available within this submenu.</summary>
    public IReadOnlyList<IMenuCommand> ChildCommands => commands;

    /// <inheritdoc/>
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
