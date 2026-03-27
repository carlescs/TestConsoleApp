namespace TestConsoleApp.Application.Abstractions;

public static class CommandRegistry
{
    private static readonly List<IMenuCommand> _commands = [];
    private static readonly IReadOnlyList<IMenuCommand> _readOnly = _commands.AsReadOnly();

    public static IReadOnlyList<IMenuCommand> Commands => _readOnly;

    public static void Register(IMenuCommand command) => _commands.Add(command);
}
