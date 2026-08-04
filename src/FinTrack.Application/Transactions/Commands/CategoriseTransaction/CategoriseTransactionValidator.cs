using FluentValidation;

namespace FinTrack.Application.Transactions.Commands.CategoriseTransaction;

public class CategoriseTransactionValidator : AbstractValidator<CategoriseTransactionCommand>
{
    public CategoriseTransactionValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("Transaction is required.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");
    }
}
