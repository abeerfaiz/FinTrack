using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;

namespace FinTrack.Infrastructure.OpenBanking;

public class CategoryRulesEngine : ICategoryRulesEngine
{
    private readonly ICategoryRuleRepository _categoryRuleRepository;

    public CategoryRulesEngine(ICategoryRuleRepository categoryRuleRepository)
    {
        _categoryRuleRepository = categoryRuleRepository;
    }

    public async Task<Guid?> FindMatchAsync(
        Guid userId,
        string? merchantName,
        string description,
        CancellationToken cancellationToken = default)
    {
        // Nothing to match against
        if (string.IsNullOrWhiteSpace(merchantName) && string.IsNullOrWhiteSpace(description))
            return null;

        var rules = await _categoryRuleRepository
            .GetByUserIdAsync(userId, cancellationToken);

        if (!rules.Any())
            return null;

        // Build the text to match against — prefer merchant name,
        // fall back to description. Both uppercased to match stored keywords.
        var matchText = (merchantName ?? description).ToUpperInvariant();

        // Rules are already ordered by priority ASC from the repository.
        // First match wins — lowest priority number = highest priority.
        foreach (var rule in rules)
        {
            if (matchText.Contains(rule.Keyword, StringComparison.OrdinalIgnoreCase))
                return rule.CategoryId;
        }

        return null;
    }
}