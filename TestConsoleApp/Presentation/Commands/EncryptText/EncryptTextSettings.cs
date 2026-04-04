using System.ComponentModel;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands.EncryptText;

/// <summary>CLI settings for <see cref="EncryptTextCommand"/>.</summary>
public sealed class EncryptTextSettings : MenuCommandSettings
{
    [CommandOption("-t|--text <text>")]
    [Description("The text to encrypt, or the Base64 ciphertext to decrypt.")]
    public string? Text { get; init; }

    [CommandOption("-k|--key <passphrase>")]
    [Description("The passphrase used to derive the encryption key.")]
    public string? Key { get; init; }

    [CommandOption("-a|--algorithm <algorithm>")]
    [Description("Encryption algorithm: aes-256-cbc, aes-256-gcm, or chacha20-poly1305 (default: aes-256-cbc).")]
    public string? Algorithm { get; init; }

    [CommandOption("-d|--decrypt")]
    [Description("Decrypt the input instead of encrypting it.")]
    public bool Decrypt { get; init; }
}
