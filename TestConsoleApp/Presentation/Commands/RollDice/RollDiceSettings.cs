using System.ComponentModel;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands.RollDice;

/// <summary>CLI settings for <see cref="RollDiceCommand"/>.</summary>
public sealed class RollDiceSettings : MenuCommandSettings
{
    [CommandOption("-s|--sides <n>")]
    [Description("Number of sides on each die (default: 6, min: 2).")]
    public int? Sides { get; init; }

    [CommandOption("-n|--num-dice <n>")]
    [Description("Number of dice to roll (default: 1, min: 1).")]
    public int? NumDice { get; init; }

    [CommandOption("-t|--throws <n>")]
    [Description("Number of dice throws to perform automatically before entering interactive mode (default: 0, min: 0).")]
    public int? InitialThrows { get; init; }
}
