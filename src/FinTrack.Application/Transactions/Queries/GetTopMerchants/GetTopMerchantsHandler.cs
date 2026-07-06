using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Transactions.Queries.GetTopMerchants;

public class GetTopMerchantsHandler
    : IRequestHandler<GetTopMerchantsQuery, Result<IReadOnlyList<MerchantSpendDto>>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTopMerchantsHandler(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<MerchantSpendDto>>> Handle(
        GetTopMerchantsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var monthStart = new DateOnly(request.Year, request.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var transactions = await _transactionRepository.GetByUserIdAsync(
            userId,
            from: monthStart,
            to: monthEnd,
            cancellationToken: cancellationToken);

        var top = Math.Clamp(request.Top, 1, 20);

        // Group by merchant name or fall back to description
        // when merchant_name is null — consistent with the rules engine
        var result = transactions
            .Where(t => t.Amount < 0)
            .GroupBy(t => t.MerchantName ?? t.Description)
            .Select(g => new MerchantSpendDto(
                MerchantName: g.Key,
                TotalSpend: Math.Abs(g.Sum(t => t.Amount)),
                TransactionCount: g.Count()))
            .OrderByDescending(m => m.TotalSpend)
            .Take(top)
            .ToList();

        return Result.Success<IReadOnlyList<MerchantSpendDto>>(result);
    }
}