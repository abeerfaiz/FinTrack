using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Exceptions;
using MediatR;

namespace FinTrack.Application.Transactions.Commands.CategoriseTransaction;

public class CategoriseTransactionHandler : IRequestHandler<CategoriseTransactionCommand, Result>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CategoriseTransactionHandler(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        CategoriseTransactionCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var transaction = await _transactionRepository
            .GetByIdAsync(request.TransactionId, cancellationToken);

        if (transaction is null)
            throw new EntityNotFoundException(nameof(Transaction), request.TransactionId);

        // IDOR check — users can only categorise their own transactions
        if (transaction.UserId != userId)
            throw new UnauthorizedAccessException(
                "You do not have permission to categorise this transaction.");

        // Verify the category exists and is available to this user
        var category = await _categoryRepository
            .GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
            throw new EntityNotFoundException(nameof(Category), request.CategoryId);

        if (category.UserId != null && category.UserId != userId)
            throw new UnauthorizedAccessException(
                "You do not have permission to use this category.");

        // isManual: true — this is a deliberate user action.
        // The domain entity's AssignCategory method will set
        // is_manually_categorised = true, permanently protecting
        // this transaction from automatic rule overrides.
        transaction.AssignCategory(request.CategoryId, isManual: true);

        _transactionRepository.Update(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}