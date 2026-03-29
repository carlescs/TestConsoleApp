using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Menus;

internal static class MenuInteraction
{
    private const int BadgeColumnWidth = 8;

    [ExcludeFromCodeCoverage]
    internal static IMenuCommand? Show(
        string header,
        IReadOnlyList<IMenuCommand> commands,
        string exitLabel,
        Action? banner = null)
    {
        var hotkeys = commands.Select(CommandRegistry.GetHotkey).ToArray();
        var selectedIndex = 0;
        var totalItems = commands.Count + 1;

        while (true)
        {
            Render(header, commands, hotkeys, exitLabel, selectedIndex, banner);
            var keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.UpArrow) { selectedIndex = (selectedIndex - 1 + totalItems) % totalItems; continue; }
            if (keyInfo.Key == ConsoleKey.DownArrow) { selectedIndex = (selectedIndex + 1) % totalItems; continue; }
            if (keyInfo.Key == ConsoleKey.Enter) return selectedIndex == commands.Count ? null : commands[selectedIndex];
            if (keyInfo.Key == ConsoleKey.Escape) return null;

            // Local hotkey dispatch
            for (var i = 0; i < hotkeys.Length; i++)
                if (hotkeys[i].HasValue && HotkeyMatches(keyInfo, hotkeys[i]!.Value.Key, hotkeys[i]!.Value.Modifiers))
                    return commands[i];

            // Global hotkey dispatch (cross-menu-level)
            var globalCmd = CommandRegistry.FindGlobalCommand(keyInfo);
            if (globalCmd is not null) return globalCmd;
        }
    }

    internal static bool HotkeyMatches(ConsoleKeyInfo keyInfo, char hotkey, ConsoleModifiers required)
    {
        var pressedMods = keyInfo.Modifiers & ~ConsoleModifiers.Shift;
        var requiredMods = required & ~ConsoleModifiers.Shift;
        if (pressedMods != requiredMods) return false;
        char effectiveChar = (pressedMods & (ConsoleModifiers.Control | ConsoleModifiers.Alt)) != 0
            ? (char)keyInfo.Key : keyInfo.KeyChar;
        return char.ToUpperInvariant(effectiveChar) == char.ToUpperInvariant(hotkey);
    }

    [ExcludeFromCodeCoverage]
    private static void Render(string header, IReadOnlyList<IMenuCommand> commands,
        (char Key, ConsoleModifiers Modifiers)?[] hotkeys, string exitLabel, int selectedIndex, Action? banner)
    {
        AnsiConsole.Clear();
        banner?.Invoke();
        AnsiConsole.Write(new Rule($"[bold]{Markup.Escape(header)}[/]").RuleStyle("blue dim"));

        for (var i = 0; i < commands.Count; i++)
        {
            var hk = hotkeys[i];
            var badgeMarkup = hk.HasValue ? BuildBadgeMarkup(hk.Value.Key, hk.Value.Modifiers) : new string(' ', BadgeColumnWidth);
            var title = Markup.Escape(commands[i].Title);
            var isSelected = i == selectedIndex;
            AnsiConsole.MarkupLine(isSelected ? $" [bold]>[/] {badgeMarkup} [bold]{title}[/]" : $"   {badgeMarkup} {title}");
        }

        var exitSelected = selectedIndex == commands.Count;
        var exitPad = new string(' ', BadgeColumnWidth);
        AnsiConsole.MarkupLine(exitSelected
            ? $" [bold]>[/] {exitPad} [bold]{Markup.Escape(exitLabel)}[/]"
            : $"   {exitPad} {Markup.Escape(exitLabel)}");

        AnsiConsole.MarkupLine("\n[dim]↑↓ navigate  Enter select  Esc back  ^=Ctrl ~=Alt  hotkey/global dispatch[/]");
    }

    internal static string BuildBadgeMarkup(char key, ConsoleModifiers modifiers)
    {
        var content = BuildBadgeContent(modifiers, key);
        var padding = new string(' ', BadgeColumnWidth - content.Length - 2);
        return $"[[[bold cyan]{Markup.Escape(content)}[/]]]{padding}";
    }

    internal static string BuildBadgeContent(ConsoleModifiers modifiers, char key)
    {
        var sb = new System.Text.StringBuilder();
        if ((modifiers & ConsoleModifiers.Control) != 0) sb.Append('^');
        if ((modifiers & ConsoleModifiers.Alt) != 0) sb.Append('~');
        sb.Append(char.ToUpperInvariant(key));
        return sb.ToString();
    }
}
