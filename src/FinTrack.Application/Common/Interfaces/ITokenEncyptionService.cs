namespace FinTrack.Application.Common.Interfaces;

/// <summary>
/// Encrypts and decrypts TrueLayer access/refresh tokens before they
/// touch the database. Implemented in Infrastructure using AES-256.
/// Application never sees the encryption algorithm or key management —
/// it just asks for plaintext in, ciphertext out, and vice versa.
/// </summary>
public interface ITokenEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}