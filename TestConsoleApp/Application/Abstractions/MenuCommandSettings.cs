using Spectre.Console.Cli;

namespace TestConsoleApp.Application.Abstractions;

/// <summary>
/// Base settings class for all CLI commands registered via <c>CliCommandBuilder</c>.
/// Commands that require their own CLI parameters should derive their settings from this class.
/// </summary>
public class MenuCommandSettings : CommandSettings
{
}
