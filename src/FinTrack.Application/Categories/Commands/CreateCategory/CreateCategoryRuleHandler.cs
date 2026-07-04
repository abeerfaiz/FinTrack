using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using FinTrack.Domain.Entities;
using MediatR;

namespace FinTrack.Application.Categories.Commands.CreateCategory;

public class CreateCategoryRuleHandler
    : IRequestHandler<CreateCategoryRuleCommand, Result<Guid>>
{
    private readonly ICategoryRuleRepository _categoryRuleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryRuleHandler(
        ICategoryRuleRepository categoryRuleRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _categoryRuleRepository = categoryRuleRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateCategoryRuleCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var rule = new CategoryRule(
            userId: userId,
            categoryId: request.CategoryId,
            keyword: request.Keyword,
            priority: request.Priority);

        await _categoryRuleRepository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(rule.Id);
    }
}