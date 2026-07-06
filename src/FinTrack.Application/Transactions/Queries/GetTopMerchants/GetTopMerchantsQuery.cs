using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Transactions.Queries.GetTopMerchants;

public record GetTopMerchantsQuery(
    int Year,
    int Month,
    int Top = 5) : IRequest<Result<IReadOnlyList<MerchantSpendDto>>>;

public record MerchantSpendDto(
    string MerchantName,
    decimal TotalSpend,
    int TransactionCount);