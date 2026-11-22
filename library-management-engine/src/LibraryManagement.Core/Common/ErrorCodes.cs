namespace LibraryManagement.Core.Common;

/// <summary>
/// Stable identifiers for every expected business failure. Callers and tests branch on
/// these codes, never on message wording, so messages can be reworded safely.
/// </summary>
public static class ErrorCodes
{
    // Catalogue
    public const string DuplicateBook = "book.duplicate";
    public const string BookNotFound = "book.not_found";
    public const string TooManyCopies = "book.too_many_copies";

    // Members
    public const string DuplicateEmail = "member.duplicate_email";
    public const string MemberNotFound = "member.not_found";

    // Borrowing
    public const string CategoryNotLoanable = "borrow.category_not_loanable";
    public const string NoCopiesAvailable = "borrow.no_copies_available";
    public const string LoanLimitReached = "borrow.loan_limit_reached";
    public const string HasOverdueLoans = "borrow.has_overdue_loans";
    public const string FinesTooHigh = "borrow.fines_too_high";
    public const string AlreadyBorrowed = "borrow.already_borrowed";

    // Loans
    public const string LoanNotFound = "loan.not_found";
    public const string LoanAlreadyReturned = "loan.already_returned";
    public const string RenewalLimitReached = "loan.renewal_limit_reached";
    public const string RenewalOverdue = "loan.renewal_overdue";
    public const string RenewalNotAllowedForCategory = "loan.renewal_not_allowed_for_category";

    // Fines
    public const string InvalidPaymentAmount = "fine.invalid_amount";
    public const string OverPayment = "fine.overpayment";
}
