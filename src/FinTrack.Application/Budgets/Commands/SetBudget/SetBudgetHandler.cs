using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using FinTrack.Domain.Entities;
using MediatR;

namespace FinTrack.Application.Budgets.Commands.SetBudget;

public class SetBudgetHandler : IRequestHandler<SetBudgetCommand, Result<Guid>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SetBudgetHandler(
        IBudgetRepository budgetRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        SetBudgetCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        // Verify the category exists and is accessible to this user
        var category = await _categoryRepository
            .GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
            return Result.Failure<Guid>("Category not found.");

        if (category.UserId != null && category.UserId != userId)
            return Result.Failure<Guid>("You do not have access to this category.");

        // Normalise to first of month — the domain entity does this
        // too, but we also check here so the existing budget lookup
        // uses the same normalised date.
        var monthStart = new DateOnly(
            request.MonthStart.Year,
            request.MonthStart.Month,
            1);

        // Upsert — if budget exists for this month/category, update it.
        // If not, create a new one.
        var existing = await _budgetRepository.GetByUserCategoryMonthAsync(
            userId, request.CategoryId, monthStart, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateAmount(request.Amount);
            _budgetRepository.Update(existing);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(existing.Id);
        }

        var budget = new Budget(
            userId: userId,
            categoryId: request.CategoryId,
            amount: request.Amount,
            monthStart: monthStart);

        await _budgetRepository.AddAsync(budget, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(budget.Id);
    }
}