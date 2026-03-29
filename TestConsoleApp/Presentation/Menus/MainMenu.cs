using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Menus;

/// <summary>
/// Presents the top-level interactive menu using Spectre.Console, looping until the user
/// chooses to exit or the cancellation token is cancelled.
/// </summary>
public sealed class MainMenu
{
    private const string ExitOption = "Exit";
    private readonly IReadOnlyList<IMenuCommand> _commands;
    private readonly IMenuInteraction _interaction;

    /// <param name="commands">The commands to display at the root level of the menu.</param>
    public MainMenu(IEnumerable<IMenuCommand> commands)
        : this(commands, DefaultMenuInteraction.Instance) { }

    internal MainMenu(IEnumerable<IMenuCommand> commands, IMenuInteraction interaction)
    {
        _commands = commands.ToList();
        _interaction = interaction;
    }

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
            var command = _interaction.Show("Main Menu", _commands, ExitOption,
                static () => AnsiConsole.Write(new FigletText("TestConsoleApp").Centered().Color(Color.Blue)));
            if (command is null)
                break;

            await command.ExecuteAsync(cancellationToken);
        }
    }
}
