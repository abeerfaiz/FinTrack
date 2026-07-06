using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Budgets.Commands.SetBudget;

/// <summary>
/// Creates or updates a budget for a specific category and month.
/// If a budget already exists for this user/category/month combination,
/// it updates the amount. If not, it creates a new one.
/// This upsert behaviour means the UI doesn't need a separate
/// "edit budget" endpoint — SetBudget handles both cases.
/// </summary>
public record SetBudgetCommand(
    Guid CategoryId,
    decimal Amount,
    DateOnly MonthStart) : IRequest<Result<Guid>>;