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
            var command = MenuInteraction.Show(title, commands, BackOption);
            if (command is null)
                break;

            await command.ExecuteAsync(cancellationToken);
        }
    }
}
