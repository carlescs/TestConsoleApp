using System.Text.RegularExpressions;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Menus;

namespace TestConsoleApp.Presentation.Cli;

/// <summary>
/// Configures a Spectre.Console CLI <see cref="IConfigurator"/> from a list of
/// <see cref="IMenuCommand"/> instances, mapping <see cref="SubMenuCommand"/> trees to CLI
/// branches and leaf commands to inline delegates.
/// </summary>
internal static partial class CliCommandBuilder
{
    /// <summary>
    /// Registers all commands from <paramref name="commands"/> into the root
    /// <paramref name="config"/>, recursively expanding any <see cref="SubMenuCommand"/>
    /// into CLI branches.
    /// </summary>
    /// <param name="config">The root Spectre.Console CLI configurator.</param>
    /// <param name="commands">The commands to register.</param>
    internal static void Configure(IConfigurator config, IReadOnlyList<IMenuCommand> commands)
    {
        foreach (var command in commands)
        {
            if (command is SubMenuCommand subMenu)
            {
                var captured = subMenu;
                config.AddBranch<EmptyCommandSettings>(ToKebabCase(captured.Title), branch =>
                    ConfigureBranch(branch, captured.ChildCommands));
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

    /// <summary>
    /// Recursively registers <paramref name="commands"/> into a nested branch configurator.
    /// </summary>
    /// <param name="config">The branch-level configurator to populate.</param>
    /// <param name="commands">The commands to register at this nesting level.</param>
    private static void ConfigureBranch(IConfigurator<EmptyCommandSettings> config, IReadOnlyList<IMenuCommand> commands)
    {
        foreach (var command in commands)
        {
            if (command is SubMenuCommand subMenu)
            {
                var captured = subMenu;
                config.AddBranch(ToKebabCase(captured.Title), branch =>
                    ConfigureBranch(branch, captured.ChildCommands));
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

    /// <summary>Converts a human-readable title into a kebab-case CLI command name.</summary>
    /// <param name="title">The title to convert.</param>
    /// <returns>A lowercase, hyphen-separated string suitable for use as a CLI verb.</returns>
    private static string ToKebabCase(string title) =>
        NonAlphanumericRegex().Replace(title.Trim().ToLowerInvariant(), "-").Trim('-');

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();
}
