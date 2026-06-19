namespace FinTrack.Domain.Enums;

/// <summary>
/// Mirrors TrueLayer's account_type field exactly.
/// Used to filter account types in the UI and determine
/// which accounts are eligible for budget calculations
/// (typically TRANSACTION accounts only).
/// </summary>
public enum AccountType
{
    Transaction,
    Savings,
    BusinessTransaction,
    BusinessSavings
}