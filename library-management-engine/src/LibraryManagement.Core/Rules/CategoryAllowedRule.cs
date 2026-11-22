using LibraryManagement.Core.Common;
using LibraryManagement.Core.Domain;

namespace LibraryManagement.Core.Rules;

/// <summary>Reference items never leave the library; some memberships are limited further.</summary>
public sealed class CategoryAllowedRule : IBorrowingRule
{
    public DomainError? Check(BorrowingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var (member, book) = (context.Member, context.Book);

        if (member.Policy.MayBorrow(book.Category))
        {
            return null;
        }

        var message = book.Category == BookCategory.Reference
            ? $"'{book.Title}' is a reference book and cannot be borrowed."
            : $"{member.MembershipType} members cannot borrow {book.Category} items such as '{book.Title}'.";

        return new DomainError(ErrorCodes.CategoryNotLoanable, message);
    }
}
