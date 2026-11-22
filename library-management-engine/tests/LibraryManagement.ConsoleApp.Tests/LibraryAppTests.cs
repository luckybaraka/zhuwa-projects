using LibraryManagement.ConsoleApp.Time;
using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Services;

namespace LibraryManagement.ConsoleApp.Tests;

/// <summary>
/// End-to-end tests: scripted keyboard input in, full console transcript out, exactly as a
/// user would drive the app. The clock is frozen at 2026-11-02 so dates are predictable.
/// </summary>
public class LibraryAppTests
{
    private readonly AdjustableClock _clock = new(new FixedClock());
    private readonly LibraryService _library;

    public LibraryAppTests() => _library = LibraryService.CreateInMemory(_clock);

    private (int ExitCode, string Output) Run(params string[] lines)
    {
        using var input = new StringReader(string.Join(Environment.NewLine, lines));
        using var output = new StringWriter();
        var exitCode = new LibraryApp(_library, _clock, input, output).Run();
        return (exitCode, output.ToString());
    }

    private (int ExitCode, string Output) RunWithSampleData(params string[] lines)
    {
        SampleData.Load(_library);
        return Run(lines);
    }

    // ---------------------------------------------------------------- Menu loop

    [Fact]
    public void Exit_SaysGoodbyeWithExitCodeZero()
    {
        var (exitCode, output) = Run("0");

        Assert.Equal(0, exitCode);
        Assert.Contains("=== Main menu | today: 2026-11-02 ===", output, StringComparison.Ordinal);
        Assert.EndsWith("Goodbye." + Environment.NewLine, output, StringComparison.Ordinal);
    }

    [Fact]
    public void InvalidMenuChoice_IsExplainedAndAskedAgain()
    {
        var (_, output) = Run("99", "abc", "0");

        Assert.Equal(2, Count(output, "Choose a number from the menu (0-13)."));
        Assert.Contains("Goodbye.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void MenuRepeatsAfterEachOperation()
    {
        var (_, output) = Run("2", "5", "0");

        Assert.Equal(3, Count(output, "=== Main menu"));
    }

    [Fact]
    public void ClosedInput_ExitsCleanlyInsteadOfLooping()
    {
        var (exitCode, output) = Run("4", "Amina");    // input ends mid-registration

        Assert.Equal(0, exitCode);
        Assert.Contains("Input closed. Goodbye.", output, StringComparison.Ordinal);
        Assert.Empty(_library.ListMembers());
    }

    [Fact]
    public void CancelInsideAnOperation_ReturnsToMenuWithoutChanges()
    {
        var (_, output) = Run("4", "Amina Otieno", "cancel", "0");

        Assert.Contains("Cancelled. Nothing was changed.", output, StringComparison.Ordinal);
        Assert.Empty(_library.ListMembers());
    }

    // ---------------------------------------------------------------- Members

    [Fact]
    public void RegisterMember_BlankName_IsAskedAgainNotAbandoned()
    {
        var (_, output) = Run("4", "", "  ", "Amina Otieno", "amina@example.org", "1", "0");

        Assert.Equal(2, Count(output, "This cannot be blank."));
        Assert.Contains("registered Amina Otieno as M-0001 (Student)", output, StringComparison.Ordinal);
        Assert.Equal("Amina Otieno", Assert.Single(_library.ListMembers()).Name);
    }

    [Fact]
    public void RegisterMember_InvalidOrDuplicateEmail_AsksOnlyForTheEmailAgain()
    {
        var (_, output) = RunWithSampleData(
            "4", "Wanjiku Kamau", "not-an-email", "AMINA.OTIENO@example.org", "wanjiku@example.org", "staff", "0");

        Assert.Contains("does not look like an e-mail address", output, StringComparison.Ordinal);
        Assert.Contains("already registered to Amina Otieno (M-0001)", output, StringComparison.Ordinal);
        Assert.Contains("registered Wanjiku Kamau as M-0004 (Staff)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void ListMembers_ShowsLoansAndFines()
    {
        var (_, output) = RunWithSampleData("5", "0");

        Assert.Contains("Brian Mwangi", output, StringComparison.Ordinal);
        Assert.Contains("0/10", output, StringComparison.Ordinal);
        Assert.Contains("KES 0.00", output, StringComparison.Ordinal);
    }

    [Fact]
    public void MemberAccount_UnknownId_IsAskedAgain()
    {
        var (_, output) = RunWithSampleData("6", "M-0099", "M-0002", "0");

        Assert.Contains("There is no member M-0099.", output, StringComparison.Ordinal);
        Assert.Contains("Brian Mwangi (M-0002), Staff member", output, StringComparison.Ordinal);
        Assert.Contains("No loans yet.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void MemberAccount_ShowsOverdueItemsBlockedStatusAndHistory()
    {
        var (_, output) = RunWithSampleData(
            "7", "M-0001", "9780132350884",
            "7", "M-0001", "9780201633610",
            "13", "30",                     // both 16 days late
            "8", "L-0001",                  // KES 160 fine -> blocked (> KES 100)
            "6", "M-0001",
            "0");

        Assert.Contains("On loan:     1 of 3 (1 overdue)", output, StringComparison.Ordinal);
        Assert.Contains("Fines owed:  KES 160.00 (borrowing blocked above KES 100.00)", output, StringComparison.Ordinal);
        Assert.Contains("Loan history:", output, StringComparison.Ordinal);
        Assert.Contains("returned 2026-12-02", output, StringComparison.Ordinal);
        Assert.Contains("OVERDUE 16 days", output, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- Catalogue

    [Fact]
    public void AddCopies_BookAtMaximum_ExplainsAndAddsNothing()
    {
        var (_, output) = Run(
            "1", "9780132350884", "Clean Code", "Robert C. Martin", "1", "100",
            "1", "9780132350884",
            "0");

        Assert.Contains("It already has the maximum of 100 copies.", output, StringComparison.Ordinal);
        Assert.Equal(100, _library.FindBook(Isbn.Parse("9780132350884"))!.Book.TotalCopies);
    }

    [Fact]
    public void AddBook_NewTitle_ThenSameIsbnAddsCopies()
    {
        var (_, output) = Run(
            "1", "12345", "978-0-13-235088-4", "Clean Code", "Robert C. Martin", "1", "2",
            "1", "0132350882", "3",
            "0");

        Assert.Contains("not a valid ISBN", output, StringComparison.Ordinal);
        Assert.Contains("added 'Clean Code' (General, 2 copies)", output, StringComparison.Ordinal);
        Assert.Contains("Already catalogued: 'Clean Code'", output, StringComparison.Ordinal);
        Assert.Contains("'Clean Code' now has 5 copies.", output, StringComparison.Ordinal);
        Assert.Equal(5, _library.FindBook(Isbn.Parse("9780132350884"))!.Book.TotalCopies);
    }

    [Fact]
    public void SearchBooks_ByAuthorFragment()
    {
        var (_, output) = RunWithSampleData("3", "fowler", "3", "tolkien", "0");

        Assert.Contains("Refactoring (2nd ed.)", output, StringComparison.Ordinal);
        Assert.Contains("No books match 'tolkien'.", output, StringComparison.Ordinal);
    }

    // ---------------------------------------------------------------- Circulation

    [Fact]
    public void FullLoanLifecycle_BorrowOverdueReturnAndPay()
    {
        var (_, output) = RunWithSampleData(
            "7", "M-0001", "9780132350884",   // borrow
            "13", "30",                         // 30 days pass: 16 days late
            "11",                               // overdue report
            "8", "L-0001",                      // return
            "10", "M-0001", "60",               // pay part of the KES 160 fine
            "0");

        Assert.Contains("Loan L-0001 is due back on 2026-11-16 (14 days)", output, StringComparison.Ordinal);
        Assert.Contains("today is now 2026-12-02", output, StringComparison.Ordinal);
        Assert.Contains("1 loan(s) are now overdue", output, StringComparison.Ordinal);
        Assert.Contains("KES 160.00", output, StringComparison.Ordinal);
        Assert.Contains("returned by Amina Otieno, 16 days late", output, StringComparison.Ordinal);
        Assert.Contains("Remaining balance: KES 100.00", output, StringComparison.Ordinal);
        Assert.Contains("Amina Otieno can borrow again.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Borrow_RefusedRequest_ListsEveryReason()
    {
        var (_, output) = RunWithSampleData("7", "M-0003", "978-0-13-468599-1", "0");

        Assert.Contains("'Effective Java (3rd ed.)' cannot be lent to Grace Wanjiru", output, StringComparison.Ordinal);
        Assert.Contains("Guest members cannot borrow ShortLoan items", output, StringComparison.Ordinal);
        Assert.Empty(_library.ActiveLoans());
    }

    [Fact]
    public void Borrow_UnknownBook_IsAskedAgain()
    {
        var (_, output) = RunWithSampleData("7", "M-0002", "9780201633610X", "9781234567897", "9780201633610", "0");

        Assert.Contains("not a valid ISBN", output, StringComparison.Ordinal);
        Assert.Contains("ISBN 9781234567897 is not in the catalogue", output, StringComparison.Ordinal);
        Assert.Contains("lent 'Design Patterns' to Brian Mwangi", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Renew_StaffAllowedThenGuestRefused()
    {
        var (_, output) = RunWithSampleData(
            "7", "M-0002", "9780132350884",
            "7", "M-0003", "9780201633610",
            "9", "L-0001",
            "9", "L-0002",
            "0");

        // Borrowed 2026-11-02, due 11-30; renewal adds another 28 days from the due date.
        Assert.Contains("loan L-0001 is now due on 2026-12-28 (renewal 1 of 3)", output, StringComparison.Ordinal);
        Assert.Contains("loan L-0002 cannot be renewed", output, StringComparison.Ordinal);
        Assert.Contains("Guest members cannot renew loans.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Return_AlreadyReturnedLoanId_IsAskedAgain()
    {
        var (_, output) = RunWithSampleData(
            "7", "M-0001", "9780132350884",
            "7", "M-0002", "9780132350884",
            "8", "L-0001",
            "8", "L-0001", "L-0099", "L-0002",
            "0");

        Assert.Contains("Loan L-0001 was already returned on 2026-11-02.", output, StringComparison.Ordinal);
        Assert.Contains("There is no loan L-0099.", output, StringComparison.Ordinal);
        Assert.Contains("'Clean Code' returned by Brian Mwangi, on time. No fine.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Return_StaffWithinGracePeriod_ExplainsZeroFine()
    {
        var (_, output) = RunWithSampleData("7", "M-0002", "9780132350884", "13", "30", "8", "L-0001", "0");

        Assert.Contains("2 days late", output, StringComparison.Ordinal);
        Assert.Contains("KES 0.00 (within the grace period)", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Rules_AreDescribedFromThePoliciesThemselves()
    {
        var (_, output) = Run("12", "0");

        Assert.Contains("first 3 days overdue free", output, StringComparison.Ordinal);
        Assert.Contains("general collection only", output, StringComparison.Ordinal);
        Assert.Contains("capped at KES 300.00", output, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptyLibrary_EveryScreenExplainsWhatToDoInstead()
    {
        var (_, output) = Run("2", "5", "6", "7", "8", "9", "10", "11", "0");

        Assert.Contains("The catalogue is empty.", output, StringComparison.Ordinal);
        Assert.Contains("No members yet. Use 'Register a member' first.", output, StringComparison.Ordinal);
        Assert.Contains("You need at least one member and one book", output, StringComparison.Ordinal);
        Assert.Equal(2, Count(output, "No items are on loan right now."));
        Assert.Contains("No member owes any fines.", output, StringComparison.Ordinal);
        Assert.Contains("Nothing is overdue as of 2026-11-02.", output, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new LibraryApp(null!, _clock, TextReader.Null, TextWriter.Null));
        Assert.Throws<ArgumentNullException>(() => new LibraryApp(_library, null!, TextReader.Null, TextWriter.Null));
    }

    private static int Count(string text, string value) =>
        (text.Length - text.Replace(value, string.Empty, StringComparison.Ordinal).Length) / value.Length;
}
