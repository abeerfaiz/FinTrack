using FluentValidation;

namespace FinTrack.Application.Categories.Commands.CreateCategory;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.");

        RuleFor(x => x.ColourHex)
            .NotEmpty().WithMessage("Colour is required.")
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Colour must be a valid hex colour e.g. #22C55E.");

        RuleFor(x => x.Icon)
            .NotEmpty().WithMessage("Icon is required.")
            .MaximumLength(50).WithMessage("Icon name cannot exceed 50 characters.");
    }
}