namespace TestConsoleApp.Application.Abstractions;

/// <summary>
/// A static registry that holds all <see cref="IMenuCommand"/> instances available in the
/// application. Commands are populated at startup via the source-generated module initializer.
/// </summary>
public static class CommandRegistry
{
    private static readonly List<IMenuCommand> _commands = [];
    private static readonly IReadOnlyList<IMenuCommand> _readOnly = _commands.AsReadOnly();

    /// <summary>Gets a read-only list of all registered commands in insertion order.</summary>
    public static IReadOnlyList<IMenuCommand> Commands => _readOnly;

    /// <summary>Registers a command with the application.</summary>
    /// <param name="command">The command to register.</param>
    public static void Register(IMenuCommand command) => _commands.Add(command);
}
