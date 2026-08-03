namespace FinTrack.Application.Common.Interfaces;

/// <summary>
/// Signs and verifies the OAuth2 "state" payload used in the TrueLayer
/// connect flow. The callback endpoint is AllowAnonymous (TrueLayer's
/// redirect carries no JWT), so the userId embedded in state can only
/// be trusted once its signature has been verified against a server-only
/// key — otherwise anyone could hand-craft a state naming another user's
/// userId and hijack their bank connection.
/// </summary>
public interface IOAuthStateService
{
    string Sign(string payload);
    bool Verify(string payload, string signature);
}
