using System.ComponentModel;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

/// <summary>CLI settings for <see cref="GenerateGuidCommand"/>.</summary>
public sealed class GenerateGuidSettings : MenuCommandSettings
{
    [CommandOption("-c|--count <n>")]
    [Description("Number of GUIDs to generate (default: 1, min: 1).")]
    public int? Count { get; init; }

    [CommandOption("-u|--uppercase")]
    [Description("Output GUIDs in upper-case.")]
    public bool Uppercase { get; init; }
}
