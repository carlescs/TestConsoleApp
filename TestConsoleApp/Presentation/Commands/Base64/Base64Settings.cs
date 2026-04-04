using System.ComponentModel;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands.Base64;

/// <summary>CLI settings for <see cref="Base64Command"/>.</summary>
public sealed class Base64Settings : MenuCommandSettings
{
    [CommandOption("-t|--text <text>")]
    [Description("The text to encode or decode.")]
    public string? Text { get; init; }

    [CommandOption("-d|--decode")]
    [Description("Decode the input from Base64 instead of encoding it.")]
    public bool Decode { get; init; }
}
