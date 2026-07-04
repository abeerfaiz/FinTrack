using FluentValidation;

namespace FinTrack.Application.Categories.Commands.CreateCategory;

public class CreateCategoryRuleValidator : AbstractValidator<CreateCategoryRuleCommand>
{
    public CreateCategoryRuleValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category is required.");

        RuleFor(x => x.Keyword)
            .NotEmpty().WithMessage("Keyword is required.")
            .MaximumLength(255).WithMessage("Keyword cannot exceed 255 characters.");

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0).WithMessage("Priority must be zero or positive.");
    }
}