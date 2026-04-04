namespace TestConsoleApp.Application.Abstractions;

/// <summary>
/// Provides symmetric encryption and decryption of text using a passphrase-derived key.
/// The algorithm is embedded in the ciphertext blob, so callers do not need to track it
/// separately when decrypting.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Gets the algorithm keys that this service supports, suitable for display in a
    /// selection prompt. Each key is accepted by <see cref="Encrypt"/>.
    /// </summary>
    IReadOnlyList<string> AlgorithmChoices { get; }

    /// <summary>
    /// Encrypts <paramref name="plaintext"/> using the specified <paramref name="algorithm"/>
    /// and a PBKDF2-derived key. Returns a self-describing Base64 blob whose first byte
    /// identifies the algorithm, enabling parameter-free decryption.
    /// </summary>
    string Encrypt(string plaintext, string passphrase, string algorithm = "aes-256-cbc");

    /// <summary>
    /// Decrypts a Base64 blob produced by <see cref="Encrypt"/>. The algorithm is read from
    /// the first byte of the blob so it does not need to be supplied separately.
    /// </summary>
    string Decrypt(string ciphertext, string passphrase);

    /// <summary>
    /// Returns the display name of the algorithm embedded in <paramref name="base64Ciphertext"/>
    /// (e.g. <c>"AES-256-GCM"</c>).
    /// </summary>
    string DetectAlgorithmName(string base64Ciphertext);
}
