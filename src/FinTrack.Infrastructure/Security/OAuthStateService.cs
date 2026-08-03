using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Security;
using Microsoft.Extensions.Configuration;

namespace FinTrack.Infrastructure.Security;

public class OAuthStateService : IOAuthStateService
{
    private readonly string _key;

    public OAuthStateService(IConfiguration configuration)
    {
        _key = configuration["OAuthState:Key"]
            ?? throw new InvalidOperationException(
                "OAuthState:Key is missing from configuration. " +
                "Add it via user-secrets for local development.");
    }

    public string Sign(string payload) => OAuthStateSigner.Sign(payload, _key);

    public bool Verify(string payload, string signature) =>
        OAuthStateSigner.Verify(payload, signature, _key);
}
