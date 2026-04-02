using System.ComponentModel;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands.SayHello;

/// <summary>CLI settings for <see cref="SayHelloCommand"/>.</summary>
public sealed class SayHelloSettings : MenuCommandSettings
{
    [CommandOption("-n|--name <name>")]
    [Description("Name to greet. Omit to be prompted interactively.")]
    public string? Name { get; init; }
}
