using System.ComponentModel;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

/// <summary>CLI settings for <see cref="RollDiceCommand"/>.</summary>
public sealed class RollDiceSettings : MenuCommandSettings
{
    [CommandOption("-s|--sides <n>")]
    [Description("Number of sides on each die (default: 6, min: 2).")]
    public int? Sides { get; init; }

    [CommandOption("-n|--num-dice <n>")]
    [Description("Number of dice to roll (default: 1, min: 1).")]
    public int? NumDice { get; init; }
}
