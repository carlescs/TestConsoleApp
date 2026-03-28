using System.Text.RegularExpressions;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Menus;

namespace TestConsoleApp.Presentation.Cli;

internal static partial class CliCommandBuilder
{
    internal static void Configure(IConfigurator config, IReadOnlyList<IMenuCommand> commands)
    {
        foreach (var command in commands)
        {
            if (command is SubMenuCommand subMenu)
            {
                var captured = subMenu;
                config.AddBranch(ToKebabCase(captured.Title), branch =>
                {
                    foreach (var child in captured.ChildCommands)
                    {
                        var c = child;
                        branch.AddDelegate<EmptyCommandSettings>(
                            ToKebabCase(c.Title),
                            (_, _, ct) =>
                            {
                                c.ExecuteAsync(ct).GetAwaiter().GetResult();
                                return 0;
                            })
                            .WithDescription(c.Title);
                    }
                });
            }
            else
            {
                var c = command;
                config.AddDelegate<EmptyCommandSettings>(
                    ToKebabCase(c.Title),
                    (_, _, ct) =>
                    {
                        c.ExecuteAsync(ct).GetAwaiter().GetResult();
                        return 0;
                    })
                    .WithDescription(c.Title);
            }
        }
    }

    private static string ToKebabCase(string title) =>
        NonAlphanumericRegex().Replace(title.Trim().ToLowerInvariant(), "-").Trim('-');

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();
}
