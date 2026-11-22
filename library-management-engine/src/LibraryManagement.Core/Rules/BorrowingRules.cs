using LibraryManagement.Core.Common;

namespace LibraryManagement.Core.Rules;

/// <summary>Built-in rule set and the logic that applies a rule set to a request.</summary>
public static class BorrowingRules
{
    /// <summary>The library's standard rules, in the order their messages are shown.</summary>
    public static IReadOnlyList<IBorrowingRule> Default { get; } =
    [
        new CategoryAllowedRule(),
        new CopyAvailableRule(),
        new NotAlreadyBorrowedRule(),
        new LoanLimitRule(),
        new NoOverdueLoansRule(),
        new FineLimitRule(),
    ];

    /// <summary>
    /// Runs every rule and collects every violation. All rules run, not just up to the
    /// first failure, so a member hears every reason at once instead of one per attempt.
    /// </summary>
    public static IReadOnlyList<DomainError> Evaluate(IEnumerable<IBorrowingRule> rules, BorrowingContext context)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(context);

        return rules
            .Select(rule => rule.Check(context))
            .OfType<DomainError>()
            .ToList();
    }
}
