using LibraryManagement.Core.Common;

namespace LibraryManagement.Core.Rules;

/// <summary>One member may hold only one copy of a title, leaving the others for everyone else.</summary>
public sealed class NotAlreadyBorrowedRule : IBorrowingRule
{
    public DomainError? Check(BorrowingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.MemberActiveLoans.Any(loan => loan.Isbn == context.Book.Isbn)
            ? new DomainError(
                ErrorCodes.AlreadyBorrowed,
                $"{context.Member.Name} already has a copy of '{context.Book.Title}' on loan.")
            : null;
    }
}
