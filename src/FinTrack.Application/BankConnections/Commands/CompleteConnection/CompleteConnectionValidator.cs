using FluentValidation;

namespace FinTrack.Application.BankConnections.Commands.CompleteConnection;

public class CompleteConnectionValidator : AbstractValidator<CompleteConnectionCommand>
{
    public CompleteConnectionValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Authorisation code is required.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.");
    }
}
