using FluentValidation;

namespace FinTrack.Application.Categories.Commands.DeleteCategoryRule;

public class DeleteCategoryRuleValidator : AbstractValidator<DeleteCategoryRuleCommand>
{
    public DeleteCategoryRuleValidator()
    {
        RuleFor(x => x.RuleId)
            .NotEmpty().WithMessage("Rule is required.");
    }
}
