using LibraryManagement.ConsoleApp.Input;
using LibraryManagement.Core.Domain;

namespace LibraryManagement.ConsoleApp.Tests.Input;

public sealed class PrompterTests : IDisposable
{
    private readonly StringWriter _output = new();

    public void Dispose() => _output.Dispose();

    private Prompter PrompterWith(params string[] lines) =>
        new(new StringReader(string.Join(Environment.NewLine, lines)), _output);

    [Fact]
    public void ReadText_BlankAnswer_AsksAgainInsteadOfGivingUp()
    {
        var prompter = PrompterWith("", "   ", "Amina Otieno");

        var name = prompter.ReadText("Full name", 100);

        Assert.Equal("Amina Otieno", name);
        Assert.Equal(2, CountOccurrences(_output.ToString(), "cannot be blank"));
        Assert.Equal(3, CountOccurrences(_output.ToString(), "Full name:"));
    }

    [Fact]
    public void ReadWholeNumber_OutOfRange_AsksAgainAndShowsRangeInPrompt()
    {
        var prompter = PrompterWith("0", "abc", "3");

        Assert.Equal(3, prompter.ReadWholeNumber("Copies", 1, 5));
        Assert.Contains("Copies (1-5):", _output.ToString(), StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(_output.ToString(), "Enter a whole number from 1 to 5"));
    }

    [Fact]
    public void ReadIsbn_InvalidThenValid()
    {
        var prompter = PrompterWith("123", "0-262-03384-4");

        Assert.Equal(Isbn.Parse("9780262033848"), prompter.ReadIsbn("ISBN"));
        Assert.Contains("not a valid ISBN", _output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ReadAmount_RejectsOverpaymentThenAccepts()
    {
        var prompter = PrompterWith("500", "40");

        Assert.Equal(40m, prompter.ReadAmount("Amount", 50m));
        Assert.Contains("more than the KES 50.00 owed", _output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ReadChoice_ListsNumberedOptions()
    {
        var prompter = PrompterWith("2");

        Assert.Equal(MembershipType.Staff, prompter.ReadChoice<MembershipType>("Type"));
        Assert.Contains("1) Student   2) Staff   3) Guest", _output.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("cancel")]
    [InlineData("  CANCEL ")]
    public void CancelWord_AbortsTheOperation(string input)
    {
        var prompter = PrompterWith(input);

        Assert.Throws<InputCancelledException>(() => prompter.ReadText("Title", 50));
    }

    [Fact]
    public void ClosedInput_ThrowsEndOfInputRatherThanLoopingForever()
    {
        var prompter = PrompterWith("", "");   // two blank answers then end of stream

        Assert.Throws<EndOfInputException>(() => prompter.ReadText("Title", 50));
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new Prompter(null!, _output));
        Assert.Throws<ArgumentNullException>(() => new Prompter(new StringReader(""), null!));
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        for (var index = text.IndexOf(value, StringComparison.Ordinal);
             index >= 0;
             index = text.IndexOf(value, index + value.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }
}
