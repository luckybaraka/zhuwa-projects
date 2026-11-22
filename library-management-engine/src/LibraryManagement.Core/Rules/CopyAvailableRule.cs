using LibraryManagement.Core.Common;

namespace LibraryManagement.Core.Rules;

/// <summary>At least one copy must be on the shelf.</summary>
public sealed class CopyAvailableRule : IBorrowingRule
{
    public DomainError? Check(BorrowingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.AvailableCopies > 0
            ? null
            : new DomainError(
                ErrorCodes.NoCopiesAvailable,
                $"All {context.Book.TotalCopies} copies of '{context.Book.Title}' are on loan.");
    }
}
