namespace TestConsoleApp.Application.Abstractions;

using System.Reflection;

/// <summary>
/// A static registry that holds all <see cref="IMenuCommand"/> instances available in the
/// application. Commands are populated at startup via the source-generated module initializer.
/// </summary>
public static class CommandRegistry
{
    private static readonly List<IMenuCommand> _commands = [];
    private static readonly IReadOnlyList<IMenuCommand> _readOnly = _commands.AsReadOnly();

    // Per-type display metadata: type → (key, modifiers)
    private static readonly Dictionary<Type, (char Key, ConsoleModifiers Modifiers)> _hotkeys = [];

    // Global dispatch map: normalised (upperKey, strippedMods) → instance
    private static readonly Dictionary<(char Key, ConsoleModifiers Modifiers), IMenuCommand> _globalHotkeys = [];

    /// <summary>Gets a read-only list of all registered commands in insertion order.</summary>
    public static IReadOnlyList<IMenuCommand> Commands => _readOnly;

    /// <summary>Registers a command with the application.</summary>
    /// <param name="command">The command to register.</param>
    public static void Register(IMenuCommand command) => _commands.Add(command);

    /// <summary>
    /// Associates a hotkey with a command instance so menus can display and globally dispatch it.
    /// Shift is stripped from the lookup key so that hotkeys are matched case-insensitively.
    /// </summary>
    /// <param name="key">The hotkey character declared on the command class.</param>
    /// <param name="modifiers">The modifier keys required alongside <paramref name="key"/>.</param>
    /// <param name="command">The concrete command instance to dispatch to.</param>
    public static void RegisterHotkey(char key, ConsoleModifiers modifiers, IMenuCommand command)
    {
        _hotkeys[command.GetType()] = (key, modifiers);
        _globalHotkeys[NormaliseDescriptor(key, modifiers)] = command;
    }

    /// <summary>
    /// Returns the hotkey descriptor for <paramref name="command"/>, or <see langword="null"/>
    /// if no hotkey was registered for its type.
    /// </summary>
    public static (char Key, ConsoleModifiers Modifiers)? GetHotkey(IMenuCommand command) =>
        _hotkeys.TryGetValue(command.GetType(), out var entry) ? entry : null;

    /// <summary>
    /// Returns the description for <paramref name="command"/> from its
    /// <see cref="CommandDescriptionAttribute"/>, or <see langword="null"/> if the attribute
    /// is not present.
    /// </summary>
    public static string? GetDescription(IMenuCommand command) =>
        command.GetType().GetCustomAttribute<CommandDescriptionAttribute>()?.Description;

    /// <summary>
    /// Finds any registered command whose hotkey matches <paramref name="keyInfo"/>,
    /// regardless of which menu level the command belongs to.
    /// Ctrl and Alt are matched exactly; Shift is ignored for case-insensitive matching.
    /// </summary>
    /// <param name="keyInfo">The key press to test.</param>
    /// <returns>The matching command, or <see langword="null"/> if none is registered.</returns>
    public static IMenuCommand? FindGlobalCommand(ConsoleKeyInfo keyInfo)
    {
        _globalHotkeys.TryGetValue(NormaliseKeyInfo(keyInfo), out var cmd);
        return cmd;
    }

    // Strips Shift and upper-cases the char so lookups are case-insensitive.
    // For Ctrl/Alt presses KeyChar is a control code, so derive the letter from ConsoleKey instead.
    private static (char Key, ConsoleModifiers Modifiers) NormaliseKeyInfo(ConsoleKeyInfo ki)
    {
        var mods = ki.Modifiers & ~ConsoleModifiers.Shift;
        var ch = (mods & (ConsoleModifiers.Control | ConsoleModifiers.Alt)) != 0
            ? char.ToUpperInvariant((char)ki.Key)
            : char.ToUpperInvariant(ki.KeyChar);
        return (ch, mods);
    }

    private static (char Key, ConsoleModifiers Modifiers) NormaliseDescriptor(char key, ConsoleModifiers modifiers) =>
        (char.ToUpperInvariant(key), modifiers & ~ConsoleModifiers.Shift);
}

