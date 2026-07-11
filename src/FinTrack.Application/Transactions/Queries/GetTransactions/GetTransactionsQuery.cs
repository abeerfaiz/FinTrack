using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Transactions.Queries.GetTransactions;

public record GetTransactionsQuery(
    Guid? AccountId = null,
    Guid? CategoryId = null,
    DateOnly? From = null,
    DateOnly? To = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<TransactionDto>>>;

public record TransactionDto(
    Guid Id,
    Guid AccountId,
    string ExternalTxId,
    string Status,
    string TransactionType,
    string TransactionCategory,
    IReadOnlyList<string> TransactionClassification,
    string Description,
    string? MerchantName,
    decimal Amount,
    string Currency,
    DateTimeOffset TransactionDate,
    decimal? RunningBalance,
    Guid? UserCategoryId,
    string? UserCategoryName,
    string? UserCategoryColour,
    bool IsManuallyCategorised);