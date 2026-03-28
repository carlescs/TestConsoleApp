namespace TestConsoleApp.Application.Abstractions;

/// <summary>
/// Marks an <see cref="IMenuCommand"/> implementation as belonging to a nested sub-menu.
/// Each element in <paramref name="path"/> represents one level of nesting, so
/// <c>[SubMenu("Tools", "Advanced")]</c> places the command under <c>Tools → Advanced</c>.
/// Omitting the attribute registers the command at the root level.
/// </summary>
/// <param name="path">
/// An ordered sequence of menu segment names that form the nesting path from the root.
/// </param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SubMenuAttribute(params string[] path) : Attribute
{
    /// <summary>Gets the ordered nesting path segments supplied to the attribute.</summary>
    public string[] Path => path;
}