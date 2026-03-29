namespace TestConsoleApp.Application.Abstractions;

/// <summary>
/// Provides a human-readable description of an <see cref="IMenuCommand"/> implementation.
/// The description is shown next to the selected item in the interactive menu and is used
/// as the command summary in CLI <c>--help</c> output.
/// </summary>
/// <param name="description">A short sentence describing what the command does.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CommandDescriptionAttribute(string description) : Attribute
{
    /// <summary>Gets the description text for the command.</summary>
    public string Description => description;
}
