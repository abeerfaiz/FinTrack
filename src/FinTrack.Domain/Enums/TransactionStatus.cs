namespace FinTrack.Domain.Enums;

/// <summary>
/// Mirrors TrueLayer's distinction between pending and settled
/// transactions. Budget and spending calculations must only ever
/// consider Settled transactions — Pending amounts may change or
/// disappear entirely before they clear.
/// </summary>
public enum TransactionStatus
{
    Pending,
    Settled
}