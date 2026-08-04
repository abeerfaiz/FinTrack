using FluentValidation;

namespace FinTrack.Application.Transactions.Commands.SyncTransactions;

public class SyncTransactionsValidator : AbstractValidator<SyncTransactionsCommand>
{
    public SyncTransactionsValidator()
    {
        RuleFor(x => x.BankConnectionId)
            .NotEmpty().WithMessage("Bank connection is required.");
    }
}
