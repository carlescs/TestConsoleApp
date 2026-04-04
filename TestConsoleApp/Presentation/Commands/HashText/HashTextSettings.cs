using System.ComponentModel;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands.HashText;

/// <summary>CLI settings for <see cref="HashTextCommand"/>.</summary>
public sealed class HashTextSettings : MenuCommandSettings
{
    [CommandOption("-t|--text <text>")]
    [Description("The text to hash.")]
    public string? Text { get; init; }

    [CommandOption("-a|--algorithm <algorithm>")]
    [Description("Hash algorithm to use: md5, sha256, or sha512 (default: sha256).")]
    public string? Algorithm { get; init; }

    [CommandOption("-u|--uppercase")]
    [Description("Output the hash in upper-case.")]
    public bool Uppercase { get; init; }
}
