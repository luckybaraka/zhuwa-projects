using LibraryManagement.ConsoleApp.Output;
using LibraryManagement.Core.Domain;
using LibraryManagement.Core.Services;

namespace LibraryManagement.ConsoleApp.Tests.Output;

public class OutputTests
{
    [Fact]
    public void Table_AlignsColumnsToTheWidestCell()
    {
        var output = new StringWriter();

        Table.Write(output, ["Id", "Name"], [["M-0001", "Amina"], ["M-0002", "Brian Mwangi"]]);

        var lines = output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal("  Id      Name", lines[0]);
        Assert.Equal("  ------  ------------", lines[1]);
        Assert.Equal("  M-0001  Amina", lines[2]);
        Assert.Equal("  M-0002  Brian Mwangi", lines[3]);
    }

    [Fact]
    public void Table_ShortensVeryLongCells()
    {
        var output = new StringWriter();

        Table.Write(output, ["Title"], [[new string('x', 100)]]);

        Assert.Contains(new string('x', Table.MaxCellWidth - 3) + "...", output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(new string('x', Table.MaxCellWidth + 1), output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void Table_WithNoRows_WritesOnlyHeader()
    {
        var output = new StringWriter();

        Table.Write(output, ["A"], []);

        Assert.Equal(2, output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length);
    }

    [Theory]
    [InlineData(0, "due in 14 days")]
    [InlineData(13, "due in 1 day")]
    [InlineData(14, "due today")]
    [InlineData(15, "OVERDUE 1 day")]
    [InlineData(20, "OVERDUE 6 days")]
    public void LoanStatus_DescribesDueDateRelativeToToday(int daysAfterBorrowing, string expected)
    {
        var (library, loan) = StudentLoan();

        Assert.Equal(expected, Display.LoanStatus(loan, library.Today.AddDays(daysAfterBorrowing)));
    }

    [Fact]
    public void LoanStatus_ReturnedLoan_ShowsReturnDate()
    {
        var (library, loan) = StudentLoan();
        library.Return(loan.Id);

        Assert.Equal("returned 2026-11-02", Display.LoanStatus(loan, library.Today.AddDays(30)));
    }

    // Loans can only be created through the service, exactly as in production.
    private static (LibraryService Library, Loan Loan) StudentLoan()
    {
        var library = LibraryService.CreateInMemory(new FixedClock());
        var isbn = Isbn.Parse("9780132350884");
        library.AddBook(isbn, "Clean Code", "Robert C. Martin", BookCategory.General, 1);
        var member = library.RegisterMember("Amina", EmailAddress.Parse("a@example.org"), MembershipType.Student).Value;
        return (library, library.Borrow(member.Id, isbn).Value);
    }

    [Fact]
    public void Date_UsesIsoFormat()
    {
        Assert.Equal("2026-01-05", Display.Date(new DateOnly(2026, 1, 5)));
    }
}
