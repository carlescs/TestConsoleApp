namespace TestConsoleApp.Application.Abstractions;

/// <summary>
/// Represents a command that can be displayed and executed within a menu.
/// </summary>
public interface IMenuCommand
{
    /// <summary>Gets the display title of the command shown in the menu.</summary>
    string Title { get; }

    /// <summary>Executes the command asynchronously.</summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
