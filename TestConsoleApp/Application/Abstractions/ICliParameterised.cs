using Spectre.Console.Cli;

namespace TestConsoleApp.Application.Abstractions;

/// <summary>
/// Opt-in interface for commands that expose typed CLI settings.
/// When implemented, <c>CliCommandBuilder</c> registers the command with
/// <typeparamref name="TSettings"/> instead of <see cref="EmptyCommandSettings"/>,
/// allowing parameters to be passed directly from the command line.
/// </summary>
public interface ICliParameterised
{
    /// <summary>Gets the concrete <see cref="CommandSettings"/> type for this command.</summary>
    Type SettingsType { get; }

    /// <summary>
    /// Returns a new command instance pre-configured with the supplied
    /// <paramref name="settings"/>, bypassing any interactive prompts for
    /// values already provided.
    /// </summary>
    IMenuCommand WithSettings(CommandSettings settings);
}
