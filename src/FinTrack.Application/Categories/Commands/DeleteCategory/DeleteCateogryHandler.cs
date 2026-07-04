using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Exceptions;
using MediatR;

namespace FinTrack.Application.Categories.Commands.DeleteCategory;

public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCategoryHandler(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var category = await _categoryRepository
            .GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
            throw new EntityNotFoundException(nameof(Category), request.CategoryId);

        // IDOR check — users can only delete their own categories
        if (category.UserId != userId)
            throw new UnauthorizedAccessException(
                "You do not have permission to delete this category.");

        // Domain entity enforces system category protection
        category.Delete();

        _categoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}