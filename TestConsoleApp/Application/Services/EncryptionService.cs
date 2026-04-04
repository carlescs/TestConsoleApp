using System.Security.Cryptography;
using System.Text;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Application.Services;

/// <summary>
/// Encrypts and decrypts text using a PBKDF2-derived key. The encrypted output is a
/// self-describing Base64 blob: the first byte identifies the algorithm, followed by the
/// 16-byte salt, then algorithm-specific nonce/IV, authentication tag, and ciphertext.
/// </summary>
public sealed class EncryptionService : IEncryptionService
{
    private const byte AlgAesCbc       = 0x01;
    private const byte AlgAesGcm       = 0x02;
    private const byte AlgChaCha20     = 0x03;
    private const byte AlgAes128Gcm    = 0x04;
    private const byte AlgAes256Ccm    = 0x05;
    private const byte AlgTripleDesCbc = 0x06;

    private static readonly string[] _algorithmChoices =
    [
        "aes-128-gcm",
        "aes-256-cbc",
        "aes-256-ccm",
        "aes-256-gcm",
        "chacha20-poly1305",
        "tripledes-cbc",
    ];

    /// <inheritdoc/>
    public IReadOnlyList<string> AlgorithmChoices => _algorithmChoices;

    /// <inheritdoc/>
    public string Encrypt(string plaintext, string passphrase, string algorithm = "aes-256-cbc")
    {
        byte algByte = algorithm.ToLowerInvariant() switch
        {
            "aes-256-gcm"       => AlgAesGcm,
            "chacha20-poly1305" => AlgChaCha20,
            "aes-128-gcm"       => AlgAes128Gcm,
            "aes-256-ccm"       => AlgAes256Ccm,
            "tripledes-cbc"     => AlgTripleDesCbc,
            _                   => AlgAesCbc,
        };

        byte[] salt      = RandomNumberGenerator.GetBytes(16);
        byte[] key       = DeriveKey(passphrase, salt, KeySizeFor(algByte));
        byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);

        byte[] blob = algByte switch
        {
            AlgAesGcm       => EncryptAesGcm(plainBytes, key, salt, algByte),
            AlgChaCha20     => EncryptChaCha20(plainBytes, key, salt, algByte),
            AlgAes128Gcm    => EncryptAes128Gcm(plainBytes, key, salt, algByte),
            AlgAes256Ccm    => EncryptAes256Ccm(plainBytes, key, salt, algByte),
            AlgTripleDesCbc => EncryptTripleDesCbc(plainBytes, key, salt, algByte),
            _               => EncryptAesCbc(plainBytes, key, salt, algByte),
        };

        return Convert.ToBase64String(blob);
    }

    /// <inheritdoc/>
    public string Decrypt(string ciphertext, string passphrase)
    {
        byte[] blob  = Convert.FromBase64String(ciphertext);
        byte algByte = blob[0];
        byte[] salt  = blob[1..17];
        byte[] rest  = blob[17..];
        byte[] key   = DeriveKey(passphrase, salt, KeySizeFor(algByte));

        byte[] plainBytes = algByte switch
        {
            AlgAesGcm       => DecryptAesGcm(rest, key),
            AlgChaCha20     => DecryptChaCha20(rest, key),
            AlgAes128Gcm    => DecryptAes128Gcm(rest, key),
            AlgAes256Ccm    => DecryptAes256Ccm(rest, key),
            AlgTripleDesCbc => DecryptTripleDesCbc(rest, key),
            _               => DecryptAesCbc(rest, key),
        };

        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <inheritdoc/>
    public string DetectAlgorithmName(string base64Ciphertext)
    {
        byte[] blob = Convert.FromBase64String(base64Ciphertext);
        return FormatAlgorithmName(blob[0] switch
        {
            AlgAesGcm       => "aes-256-gcm",
            AlgChaCha20     => "chacha20-poly1305",
            AlgAes128Gcm    => "aes-128-gcm",
            AlgAes256Ccm    => "aes-256-ccm",
            AlgTripleDesCbc => "tripledes-cbc",
            _               => "aes-256-cbc",
        });
    }

    internal static string FormatAlgorithmName(string algorithmKey) => algorithmKey.ToLowerInvariant() switch
    {
        "aes-128-gcm"       => "AES-128-GCM",
        "aes-256-gcm"       => "AES-256-GCM",
        "aes-256-ccm"       => "AES-256-CCM",
        "chacha20-poly1305" => "ChaCha20-Poly1305",
        "tripledes-cbc"     => "TripleDES-CBC",
        _                   => "AES-256-CBC",
    };

    // ── AES-256-CBC ─────────────────────────────────────────────────────────
    // blob layout after alg byte + salt: [iv(16)] [ciphertext]

    private static byte[] EncryptAesCbc(byte[] plaintext, byte[] key, byte[] salt, byte algByte)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        byte[] ciphertext = aes.EncryptCbc(plaintext, aes.IV);
        byte[] blob       = new byte[1 + salt.Length + aes.IV.Length + ciphertext.Length];
        blob[0] = algByte;
        salt.CopyTo(blob, 1);
        aes.IV.CopyTo(blob, 17);
        ciphertext.CopyTo(blob, 17 + aes.IV.Length);
        return blob;
    }

    private static byte[] DecryptAesCbc(byte[] rest, byte[] key)
    {
        byte[] iv   = rest[..16];
        byte[] data = rest[16..];
        using var aes = Aes.Create();
        aes.Key = key;
        return aes.DecryptCbc(data, iv);
    }

    // ── AES-256-GCM ─────────────────────────────────────────────────────────
    // blob layout after alg byte + salt: [nonce(12)] [tag(16)] [ciphertext]

    private static byte[] EncryptAesGcm(byte[] plaintext, byte[] key, byte[] salt, byte algByte)
    {
        byte[] nonce      = RandomNumberGenerator.GetBytes(12);
        byte[] tag        = new byte[16];
        byte[] ciphertext = new byte[plaintext.Length];
        using var aesGcm  = new AesGcm(key, 16);
        aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

        byte[] blob = new byte[1 + salt.Length + nonce.Length + tag.Length + ciphertext.Length];
        blob[0] = algByte;
        salt.CopyTo(blob, 1);
        nonce.CopyTo(blob, 17);
        tag.CopyTo(blob, 17 + nonce.Length);
        ciphertext.CopyTo(blob, 17 + nonce.Length + tag.Length);
        return blob;
    }

    private static byte[] DecryptAesGcm(byte[] rest, byte[] key)
    {
        byte[] nonce     = rest[..12];
        byte[] tag       = rest[12..28];
        byte[] data      = rest[28..];
        byte[] plaintext = new byte[data.Length];
        using var aesGcm = new AesGcm(key, 16);
        aesGcm.Decrypt(nonce, data, tag, plaintext);
        return plaintext;
    }

    // ── ChaCha20-Poly1305 ────────────────────────────────────────────────────
    // blob layout after alg byte + salt: [nonce(12)] [tag(16)] [ciphertext]

    private static byte[] EncryptChaCha20(byte[] plaintext, byte[] key, byte[] salt, byte algByte)
    {
        byte[] nonce      = RandomNumberGenerator.GetBytes(12);
        byte[] tag        = new byte[16];
        byte[] ciphertext = new byte[plaintext.Length];
        using var chacha  = new ChaCha20Poly1305(key);
        chacha.Encrypt(nonce, plaintext, ciphertext, tag);

        byte[] blob = new byte[1 + salt.Length + nonce.Length + tag.Length + ciphertext.Length];
        blob[0] = algByte;
        salt.CopyTo(blob, 1);
        nonce.CopyTo(blob, 17);
        tag.CopyTo(blob, 17 + nonce.Length);
        ciphertext.CopyTo(blob, 17 + nonce.Length + tag.Length);
        return blob;
    }

    private static byte[] DecryptChaCha20(byte[] rest, byte[] key)
    {
        byte[] nonce     = rest[..12];
        byte[] tag       = rest[12..28];
        byte[] data      = rest[28..];
        byte[] plaintext = new byte[data.Length];
        using var chacha = new ChaCha20Poly1305(key);
        chacha.Decrypt(nonce, data, tag, plaintext);
        return plaintext;
    }

    // ── AES-128-GCM ──────────────────────────────────────────────────────────
    // blob layout after alg byte + salt: [nonce(12)] [tag(16)] [ciphertext]

    private static byte[] EncryptAes128Gcm(byte[] plaintext, byte[] key, byte[] salt, byte algByte)
    {
        byte[] nonce      = RandomNumberGenerator.GetBytes(12);
        byte[] tag        = new byte[16];
        byte[] ciphertext = new byte[plaintext.Length];
        using var aesGcm  = new AesGcm(key, 16);
        aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);

        byte[] blob = new byte[1 + salt.Length + nonce.Length + tag.Length + ciphertext.Length];
        blob[0] = algByte;
        salt.CopyTo(blob, 1);
        nonce.CopyTo(blob, 17);
        tag.CopyTo(blob, 17 + nonce.Length);
        ciphertext.CopyTo(blob, 17 + nonce.Length + tag.Length);
        return blob;
    }

    private static byte[] DecryptAes128Gcm(byte[] rest, byte[] key)
    {
        byte[] nonce     = rest[..12];
        byte[] tag       = rest[12..28];
        byte[] data      = rest[28..];
        byte[] plaintext = new byte[data.Length];
        using var aesGcm = new AesGcm(key, 16);
        aesGcm.Decrypt(nonce, data, tag, plaintext);
        return plaintext;
    }

    // ── AES-256-CCM ──────────────────────────────────────────────────────────
    // blob layout after alg byte + salt: [nonce(12)] [tag(16)] [ciphertext]

    private static byte[] EncryptAes256Ccm(byte[] plaintext, byte[] key, byte[] salt, byte algByte)
    {
        byte[] nonce      = RandomNumberGenerator.GetBytes(12);
        byte[] tag        = new byte[16];
        byte[] ciphertext = new byte[plaintext.Length];
        using var aesCcm  = new AesCcm(key);
        aesCcm.Encrypt(nonce, plaintext, ciphertext, tag);

        byte[] blob = new byte[1 + salt.Length + nonce.Length + tag.Length + ciphertext.Length];
        blob[0] = algByte;
        salt.CopyTo(blob, 1);
        nonce.CopyTo(blob, 17);
        tag.CopyTo(blob, 17 + nonce.Length);
        ciphertext.CopyTo(blob, 17 + nonce.Length + tag.Length);
        return blob;
    }

    private static byte[] DecryptAes256Ccm(byte[] rest, byte[] key)
    {
        byte[] nonce     = rest[..12];
        byte[] tag       = rest[12..28];
        byte[] data      = rest[28..];
        byte[] plaintext = new byte[data.Length];
        using var aesCcm = new AesCcm(key);
        aesCcm.Decrypt(nonce, data, tag, plaintext);
        return plaintext;
    }

    // ── TripleDES-CBC ────────────────────────────────────────────────────────
    // blob layout after alg byte + salt: [iv(8)] [ciphertext]

    private static byte[] EncryptTripleDesCbc(byte[] plaintext, byte[] key, byte[] salt, byte algByte)
    {
        using var des = TripleDES.Create();
        des.Key = key;
        des.GenerateIV();

        byte[] ciphertext = des.EncryptCbc(plaintext, des.IV);
        byte[] blob       = new byte[1 + salt.Length + des.IV.Length + ciphertext.Length];
        blob[0] = algByte;
        salt.CopyTo(blob, 1);
        des.IV.CopyTo(blob, 17);
        ciphertext.CopyTo(blob, 17 + des.IV.Length);
        return blob;
    }

    private static byte[] DecryptTripleDesCbc(byte[] rest, byte[] key)
    {
        byte[] iv   = rest[..8];
        byte[] data = rest[8..];
        using var des = TripleDES.Create();
        des.Key = key;
        return des.DecryptCbc(data, iv);
    }

    // ── Shared ───────────────────────────────────────────────────────────────

    private static int KeySizeFor(byte algByte) => algByte switch
    {
        AlgAes128Gcm    => 16,
        AlgTripleDesCbc => 24,
        _               => 32,
    };

    private static byte[] DeriveKey(string passphrase, byte[] salt, int keySize)
        => Rfc2898DeriveBytes.Pbkdf2(passphrase, salt, 100_000, HashAlgorithmName.SHA256, keySize);
}
