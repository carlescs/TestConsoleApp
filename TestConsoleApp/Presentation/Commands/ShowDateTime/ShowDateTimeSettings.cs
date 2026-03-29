using System.ComponentModel;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

/// <summary>CLI settings for <see cref="ShowDateTimeCommand"/>.</summary>
public sealed class ShowDateTimeSettings : MenuCommandSettings
{
    [CommandOption("-f|--format <format>")]
    [Description("Date/time format string (default: F — full date/time pattern).")]
    public string? Format { get; init; }
}
