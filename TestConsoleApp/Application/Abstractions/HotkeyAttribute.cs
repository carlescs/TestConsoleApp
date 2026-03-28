namespace TestConsoleApp.Application.Abstractions;

/// <summary>
/// Assigns a hotkey to an <see cref="IMenuCommand"/> implementation so that the command can be
/// dispatched directly — from any menu level — by pressing the designated key combination.
/// The generator reads this attribute at compile time and emits the necessary
/// <c>CommandRegistry.RegisterHotkey</c> call.
/// </summary>
/// <param name="key">The character the user must press to invoke this command.</param>
/// <param name="modifiers">
/// Optional modifier keys (e.g. <see cref="ConsoleModifiers.Control"/>) that must be held
/// simultaneously. Defaults to no modifier.
/// </param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class HotkeyAttribute(char key, ConsoleModifiers modifiers = default) : Attribute
{
    /// <summary>Gets the hotkey character assigned to this command.</summary>
    public char Key => key;

    /// <summary>Gets the modifier keys required alongside <see cref="Key"/>.</summary>
    public ConsoleModifiers Modifiers => modifiers;
}
