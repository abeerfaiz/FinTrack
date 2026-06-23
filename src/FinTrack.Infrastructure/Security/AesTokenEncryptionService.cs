using FinTrack.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace FinTrack.Infrastructure.Security;

/// <summary>
/// Encrypts and decrypts TrueLayer OAuth2 tokens using AES-256-CBC
/// before they're persisted to the database. The encryption key is
/// read from configuration (locally from user-secrets, in production
/// from Azure Key Vault) — never hardcoded, never in source control.
/// </summary>
public class AesTokenEncryptionService : ITokenEncryptionService
{
    private readonly byte[] _key;

    public AesTokenEncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException(
                "Encryption:Key is missing from configuration. " +
                "Add it via user-secrets for local development.");

        _key = Convert.FromBase64String(keyBase64);

        if (_key.Length != 32)
            throw new InvalidOperationException(
                "Encryption:Key must be a 256-bit (32-byte) AES key encoded as Base64.");
    }

    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV(); // fresh random IV for every single encryption

        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertextBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        // Prepend IV to ciphertext so Decrypt can extract it later.
        // IV is not a secret — it's safe to store alongside the ciphertext.
        // What matters is that it's unique per encryption, which GenerateIV() guarantees.
        var combined = new byte[aes.IV.Length + ciphertextBytes.Length];
        aes.IV.CopyTo(combined, 0);
        ciphertextBytes.CopyTo(combined, aes.IV.Length);

        return Convert.ToBase64String(combined);
    }

    public string Decrypt(string ciphertext)
    {
        var combined = Convert.FromBase64String(ciphertext);

        using var aes = Aes.Create();
        aes.Key = _key;

        // Extract the IV from the first 16 bytes (AES block size)
        var iv = new byte[16];
        var ciphertextBytes = new byte[combined.Length - 16];
        Array.Copy(combined, 0, iv, 0, 16);
        Array.Copy(combined, 16, ciphertextBytes, 0, ciphertextBytes.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plaintextBytes = decryptor.TransformFinalBlock(ciphertextBytes, 0, ciphertextBytes.Length);
        return Encoding.UTF8.GetString(plaintextBytes);
    }
}