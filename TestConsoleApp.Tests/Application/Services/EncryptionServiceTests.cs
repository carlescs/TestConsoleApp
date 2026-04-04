using TestConsoleApp.Application.Services;

namespace TestConsoleApp.Tests.Application.Services;

public sealed class EncryptionServiceTests
{
    private readonly EncryptionService _sut = new();

    // ── AlgorithmChoices ────────────────────────────────────────────────────

    [Fact]
    public void AlgorithmChoices_ContainsAllSupportedAlgorithms()
    {
        Assert.Contains("aes-128-gcm",       _sut.AlgorithmChoices);
        Assert.Contains("aes-256-cbc",       _sut.AlgorithmChoices);
        Assert.Contains("aes-256-ccm",       _sut.AlgorithmChoices);
        Assert.Contains("aes-256-gcm",       _sut.AlgorithmChoices);
        Assert.Contains("chacha20-poly1305", _sut.AlgorithmChoices);
        Assert.Contains("tripledes-cbc",     _sut.AlgorithmChoices);
    }

    // ── Encrypt ─────────────────────────────────────────────────────────────

    [Fact]
    public void Encrypt_ProducesDifferentOutputEachCall()
    {
        string first  = _sut.Encrypt("hello", "passphrase");
        string second = _sut.Encrypt("hello", "passphrase");

        // Random salt and nonce/IV mean the ciphertext differs every call.
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Encrypt_ReturnsValidBase64()
    {
        string result = _sut.Encrypt("hello", "passphrase");

        Assert.True(Convert.TryFromBase64String(result, new byte[result.Length], out _));
    }

    // ── Round-trip ──────────────────────────────────────────────────────────

    [Fact]
    public void EncryptThenDecrypt_RoundTrips()
    {
        const string original   = "Hello, World!";
        const string passphrase = "my-secret-key";

        string ciphertext = _sut.Encrypt(original, passphrase);
        string decrypted  = _sut.Decrypt(ciphertext, passphrase);

        Assert.Equal(original, decrypted);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("A longer string with spaces and punctuation!")]
    [InlineData("Unicode: こんにちは 🔒")]
    public void EncryptThenDecrypt_RoundTrips_ForVariousInputs(string text)
    {
        string ciphertext = _sut.Encrypt(text, "passphrase");
        string decrypted  = _sut.Decrypt(ciphertext, "passphrase");

        Assert.Equal(text, decrypted);
    }

    [Theory]
    [InlineData("aes-128-gcm")]
    [InlineData("aes-256-cbc")]
    [InlineData("aes-256-ccm")]
    [InlineData("aes-256-gcm")]
    [InlineData("chacha20-poly1305")]
    [InlineData("tripledes-cbc")]
    public void EncryptThenDecrypt_RoundTrips_ForAllAlgorithms(string algorithm)
    {
        const string original   = "Hello, algorithms!";
        const string passphrase = "test-key";

        string ciphertext = _sut.Encrypt(original, passphrase, algorithm);
        string decrypted  = _sut.Decrypt(ciphertext, passphrase);

        Assert.Equal(original, decrypted);
    }

    // ── Decrypt ─────────────────────────────────────────────────────────────

    [Fact]
    public void Decrypt_WithWrongPassphrase_Throws()
    {
        string ciphertext = _sut.Encrypt("secret", "correct-key");

        Assert.ThrowsAny<Exception>(() => _sut.Decrypt(ciphertext, "wrong-key"));
    }

    [Theory]
    [InlineData("aes-128-gcm")]
    [InlineData("aes-256-cbc")]
    [InlineData("aes-256-ccm")]
    [InlineData("aes-256-gcm")]
    [InlineData("chacha20-poly1305")]
    [InlineData("tripledes-cbc")]
    public void Decrypt_WithWrongPassphrase_Throws_ForAllAlgorithms(string algorithm)
    {
        string ciphertext = _sut.Encrypt("secret", "correct-key", algorithm);

        Assert.ThrowsAny<Exception>(() => _sut.Decrypt(ciphertext, "wrong-key"));
    }

    // ── DetectAlgorithmName ─────────────────────────────────────────────────

    [Theory]
    [InlineData("aes-128-gcm",       "AES-128-GCM")]
    [InlineData("aes-256-cbc",       "AES-256-CBC")]
    [InlineData("aes-256-ccm",       "AES-256-CCM")]
    [InlineData("aes-256-gcm",       "AES-256-GCM")]
    [InlineData("chacha20-poly1305", "ChaCha20-Poly1305")]
    [InlineData("tripledes-cbc",     "TripleDES-CBC")]
    public void DetectAlgorithmName_ReturnsDisplayName(string algorithm, string expectedDisplayName)
    {
        string ciphertext = _sut.Encrypt("test", "pass", algorithm);

        Assert.Equal(expectedDisplayName, _sut.DetectAlgorithmName(ciphertext));
    }

    // ── FormatAlgorithmName ─────────────────────────────────────────────────

    [Theory]
    [InlineData("aes-128-gcm",       "AES-128-GCM")]
    [InlineData("aes-256-cbc",       "AES-256-CBC")]
    [InlineData("aes-256-ccm",       "AES-256-CCM")]
    [InlineData("aes-256-gcm",       "AES-256-GCM")]
    [InlineData("chacha20-poly1305", "ChaCha20-Poly1305")]
    [InlineData("tripledes-cbc",     "TripleDES-CBC")]
    [InlineData("unknown",           "AES-256-CBC")]
    public void FormatAlgorithmName_ReturnsExpectedDisplayName(string key, string expected)
    {
        Assert.Equal(expected, EncryptionService.FormatAlgorithmName(key));
    }
}
