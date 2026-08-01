using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using FinTrack.Domain.Exceptions;
using MediatR;

namespace FinTrack.Application.Categories.Commands.DeleteCategoryRule;

public class DeleteCategoryRuleHandler : IRequestHandler<DeleteCategoryRuleCommand, Result>
{
    private readonly ICategoryRuleRepository _categoryRuleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryRuleHandler(
        ICategoryRuleRepository categoryRuleRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _categoryRuleRepository = categoryRuleRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteCategoryRuleCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var rule = await _categoryRuleRepository
            .GetByIdAsync(request.RuleId, cancellationToken);

        if (rule is null)
            throw new EntityNotFoundException(nameof(Domain.Entities.CategoryRule), request.RuleId);

        // IDOR check — users can only delete their own category rules
        if (rule.UserId != userId)
            throw new UnauthorizedAccessException(
                "You do not have permission to delete this category rule.");

        _categoryRuleRepository.Delete(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
