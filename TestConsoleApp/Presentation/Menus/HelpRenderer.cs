using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Menus;

/// <summary>
/// Metadata for a single command entry rendered in the help screen.
/// </summary>
/// <param name="Title">The command's display title.</param>
/// <param name="Hotkey">
/// Formatted hotkey badge content (e.g. <c>^H</c> for Ctrl+H), or <see cref="string.Empty"/>
/// when no hotkey is registered.
/// </param>
/// <param name="Description">The command description, or <see langword="null"/> if absent.</param>
internal sealed record HelpEntry(string Title, string Hotkey, string? Description);

/// <summary>
/// A group of commands that share a menu path, representing one section of the help screen.
/// </summary>
/// <param name="Heading">The menu-path label (e.g. <c>Main Menu</c> or <c>Utilities</c>).</param>
/// <param name="Entries">The leaf commands belonging to this section.</param>
internal sealed record HelpSection(string Heading, IReadOnlyList<HelpEntry> Entries);

/// <summary>
/// Builds and displays a full-screen help overlay listing every registered command grouped
/// by menu section, with its hotkey and description.
/// </summary>
internal static class HelpRenderer
{
    /// <summary>
    /// Renders the help screen to the terminal and waits for a key press before returning.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static void Show()
    {
        var sections = BuildSections(CommandRegistry.Commands);

        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[bold]Help \u2014 Available Commands[/]").RuleStyle("blue dim"));

        foreach (var section in sections)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($" [bold blue]{Markup.Escape(section.Heading)}[/]");

            var table = new Table()
                .Border(TableBorder.None)
                .HideHeaders()
                .AddColumn(new TableColumn("Hotkey").Width(6))
                .AddColumn(new TableColumn("Command").Width(22))
                .AddColumn("Description");

            foreach (var entry in section.Entries)
            {
                var hotkeyCell = entry.Hotkey.Length > 0
                    ? $"[bold cyan]{Markup.Escape(entry.Hotkey)}[/]"
                    : string.Empty;
                var descCell = entry.Description is not null
                    ? $"[dim]{Markup.Escape(entry.Description)}[/]"
                    : "[dim italic](no description)[/]";

                table.AddRow(hotkeyCell, Markup.Escape(entry.Title), descCell);
            }

            AnsiConsole.Write(table);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Press any key to close help...[/]");
        Console.ReadKey(intercept: true);
    }

    /// <summary>
    /// Walks <paramref name="commands"/>, recursing into any <see cref="SubMenuCommand"/> nodes,
    /// and returns one <see cref="HelpSection"/> per non-empty group of leaf commands.
    /// </summary>
    /// <param name="commands">The command list to inspect — typically <see cref="CommandRegistry.Commands"/>.</param>
    /// <param name="heading">Section label for the root level.</param>
    internal static IReadOnlyList<HelpSection> BuildSections(
        IReadOnlyList<IMenuCommand> commands,
        string heading = "Main Menu")
    {
        var result = new List<HelpSection>();
        CollectSections(commands, heading, result);
        return result;
    }

    private static void CollectSections(
        IReadOnlyList<IMenuCommand> commands,
        string heading,
        List<HelpSection> result)
    {
        var leaves = commands.Where(c => c is not SubMenuCommand).ToList();
        if (leaves.Count > 0)
        {
            result.Add(new HelpSection(
                heading,
                leaves.Select(c => new HelpEntry(
                    c.Title,
                    FormatHotkey(CommandRegistry.GetHotkey(c)),
                    CommandRegistry.GetDescription(c)
                )).ToList()));
        }

        foreach (var sub in commands.OfType<SubMenuCommand>())
            CollectSections(sub.ChildCommands, sub.Title, result);
    }

    private static string FormatHotkey((char Key, ConsoleModifiers Modifiers)? hotkey) =>
        hotkey.HasValue
            ? MenuInteraction.BuildBadgeContent(hotkey.Value.Modifiers, hotkey.Value.Key)
            : string.Empty;
}
