namespace FinTrack.Domain.Enums;

/// <summary>
/// Explicit transaction direction, mirroring TrueLayer's own
/// transaction_type field. Stored alongside the signed Amount
/// (negative = Debit, positive = Credit) for two reasons: the sign
/// supports arithmetic (summing nets your balance change), while
/// this explicit type supports readable filtering
/// (WHERE TransactionType = 'Debit').
/// </summary>
public enum TransactionType
{
    Debit,
    Credit
}