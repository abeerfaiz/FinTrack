using System.Security.Cryptography;
using System.Text;

namespace FinTrack.Application.Common.Security;

/// <summary>
/// Signs and verifies the OAuth2 "state" payload used in the TrueLayer
/// connect flow, so the callback (which is AllowAnonymous — TrueLayer's
/// redirect carries no JWT) can trust the userId embedded in state
/// without an attacker being able to forge it.
/// </summary>
public static class OAuthStateSigner
{
    public static string Sign(string payload, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));

        return Convert.ToBase64String(hash)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    public static bool Verify(string payload, string signature, string key)
    {
        var expected = Sign(payload, key);

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(signature);

        return expectedBytes.Length == actualBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}
