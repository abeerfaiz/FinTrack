using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Transactions.Commands.CategoriseTransaction;

public record CategoriseTransactionCommand(
    Guid TransactionId,
    Guid CategoryId) : IRequest<Result>;