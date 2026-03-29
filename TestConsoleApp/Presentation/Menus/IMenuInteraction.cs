using System.Diagnostics.CodeAnalysis;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Menus;

internal interface IMenuInteraction
{
    IMenuCommand? Show(string header, IReadOnlyList<IMenuCommand> commands, string exitLabel, Action? banner = null);
}

internal sealed class DefaultMenuInteraction : IMenuInteraction
{
    internal static readonly IMenuInteraction Instance = new DefaultMenuInteraction();

    [ExcludeFromCodeCoverage]
    public IMenuCommand? Show(string header, IReadOnlyList<IMenuCommand> commands, string exitLabel, Action? banner = null)
        => MenuInteraction.Show(header, commands, exitLabel, banner);
}
