namespace FinTrack.Domain.Enums;

public enum BankConnectionStatus
{
    Active,
    Expired,   // 90-day consent window passed — user must re-authorise
    Revoked    // user explicitly disconnected the bank
}