using FluentValidation;

namespace FinTrack.Application.Budgets.Commands.SetBudget;

public class SetBudgetValidator : AbstractValidator<SetBudgetCommand>
{
    public SetBudgetValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Budget amount must be greater than zero.");

        RuleFor(x => x.MonthStart)
            .NotEmpty().WithMessage("Month is required.")
            .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(12)))
            .WithMessage("Cannot set a budget more than 12 months in the future.");
    }
}