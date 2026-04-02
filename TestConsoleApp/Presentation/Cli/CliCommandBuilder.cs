using System.Reflection;
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
    private static readonly MethodInfo _sRegisterRootMethod =
        typeof(CliCommandBuilder).GetMethod("RegisterRootTyped", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly MethodInfo _sRegisterBranchMethod =
        typeof(CliCommandBuilder).GetMethod("RegisterBranchTyped", BindingFlags.NonPublic | BindingFlags.Static)!;
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
                config.AddBranch<MenuCommandSettings>(ToKebabCase(captured.Title), branch =>
                    ConfigureBranch(branch, captured.ChildCommands));
            }
            else
            {
                var c = command;
                string name = ToKebabCase(c.Title);
                string description = CommandRegistry.GetDescription(c) ?? c.Title;

                if (c is ICliParameterised cliParam)
                {
                    _sRegisterRootMethod.MakeGenericMethod(cliParam.SettingsType)
                        .Invoke(null, [config, name, cliParam, description]);
                }
                else
                {
                    config.AddDelegate<MenuCommandSettings>(name, (_, _, ct) =>
                    {
                        c.ExecuteAsync(ct).GetAwaiter().GetResult();
                        return 0;
                    }).WithDescription(description);
                }
            }
        }
    }
    /// <summary>
    /// Recursively registers <paramref name="commands"/> into a nested branch configurator.
    /// </summary>
    /// <param name="config">The branch-level configurator to populate.</param>
    /// <param name="commands">The commands to register at this nesting level.</param>
    private static void ConfigureBranch(IConfigurator<MenuCommandSettings> config, IReadOnlyList<IMenuCommand> commands)
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
                string name = ToKebabCase(c.Title);
                string description = CommandRegistry.GetDescription(c) ?? c.Title;

                if (c is ICliParameterised cliParam)
                {
                    _sRegisterBranchMethod.MakeGenericMethod(cliParam.SettingsType)
                        .Invoke(null, [config, name, cliParam, description]);
                }
                else
                {
                    config.AddDelegate<MenuCommandSettings>(name, (_, _, ct) =>
                    {
                        c.ExecuteAsync(ct).GetAwaiter().GetResult();
                        return 0;
                    }).WithDescription(description);
                }
            }
        }
    }

    private static void RegisterRootTyped<TSettings>(
        IConfigurator config, string name, ICliParameterised cliParam, string description)
        where TSettings : MenuCommandSettings
    {
        config.AddDelegate<TSettings>(name, (_, settings, ct) =>
        {
            cliParam.WithSettings(settings).ExecuteAsync(ct).GetAwaiter().GetResult();
            return 0;
        }).WithDescription(description);
    }

    private static void RegisterBranchTyped<TSettings>(
        IConfigurator<MenuCommandSettings> config, string name, ICliParameterised cliParam, string description)
        where TSettings : MenuCommandSettings
    {
        config.AddDelegate<TSettings>(name, (_, settings, ct) =>
        {
            cliParam.WithSettings(settings).ExecuteAsync(ct).GetAwaiter().GetResult();
            return 0;
        }).WithDescription(description);
    }

    /// <summary>Converts a human-readable title into a kebab-case CLI command name.</summary>
    /// <param name="title">The title to convert.</param>
    /// <returns>A lowercase, hyphen-separated string suitable for use as a CLI verb.</returns>
    private static string ToKebabCase(string title) =>
        NonAlphanumericRegex().Replace(title.Trim().ToLowerInvariant(), "-").Trim('-');

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();
}
