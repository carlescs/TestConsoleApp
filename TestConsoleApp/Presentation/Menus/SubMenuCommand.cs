using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Menus;

/// <summary>
/// An <see cref="IMenuCommand"/> that presents a nested submenu of child commands,
/// looping until the user selects <c>Back</c> or the cancellation token is cancelled.
/// </summary>
public sealed class SubMenuCommand : IMenuCommand
{
    private const string BackOption = "Back";
    private readonly IMenuInteraction _interaction;
    private readonly IAnsiConsole _console;

    /// <param name="title">The display title shown as the menu heading and in parent menus.</param>
    /// <param name="commands">The child commands available within this submenu.</param>
    public SubMenuCommand(string title, IReadOnlyList<IMenuCommand> commands)
        : this(title, commands, DefaultMenuInteraction.Instance) { }

    internal SubMenuCommand(string title, IReadOnlyList<IMenuCommand> commands, IMenuInteraction interaction, IAnsiConsole? console = null)
    {
        Title = title;
        ChildCommands = commands;
        _interaction = interaction;
        _console = console ?? AnsiConsole.Console;
    }

    /// <inheritdoc/>
    public string Title { get; }

    /// <summary>Gets the child commands available within this submenu.</summary>
    public IReadOnlyList<IMenuCommand> ChildCommands { get; }

    /// <inheritdoc/>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var command = _interaction.Show(Title, ChildCommands, BackOption);
            if (command is null)
                break;

            await command.ExecuteAsync(cancellationToken);
            _console.MarkupLine("\n[dim]Press any key to continue...[/]");
            _console.Input.ReadKey(intercept: true);
        }
    }
}
