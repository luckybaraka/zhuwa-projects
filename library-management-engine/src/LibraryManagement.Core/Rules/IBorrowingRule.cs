using LibraryManagement.Core.Common;

namespace LibraryManagement.Core.Rules;

/// <summary>
/// One independent condition a loan must satisfy. New rules (e.g. "no borrowing during
/// stock-take") are added as new classes, without editing the service: open for
/// extension, closed for modification.
/// </summary>
public interface IBorrowingRule
{
    /// <summary>Returns why the loan is not allowed, or <c>null</c> when this rule is satisfied.</summary>
    DomainError? Check(BorrowingContext context);
}
