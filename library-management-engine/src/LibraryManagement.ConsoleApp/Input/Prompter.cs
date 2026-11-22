using LibraryManagement.Core.Domain;

namespace LibraryManagement.ConsoleApp.Input;

/// <summary>
/// Asks questions and keeps asking until the answer is valid. Reading through
/// <see cref="TextReader"/>/<see cref="TextWriter"/> instead of <see cref="Console"/> lets
/// tests drive whole sessions with scripted input.
/// </summary>
public sealed class Prompter
{
    /// <summary>Typing this at any prompt inside an operation returns to the main menu.</summary>
    public const string CancelWord = "cancel";

    private readonly TextReader _input;
    private readonly TextWriter _output;

    public Prompter(TextReader input, TextWriter output)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>Reads one trimmed line.</summary>
    /// <exception cref="EndOfInputException">Input has been closed.</exception>
    /// <exception cref="InputCancelledException">The user typed <see cref="CancelWord"/>.</exception>
    public string ReadLine(string prompt)
    {
        _output.Write($"{prompt}: ");
        var line = _input.ReadLine() ?? throw new EndOfInputException();
        var trimmed = line.Trim();

        return string.Equals(trimmed, CancelWord, StringComparison.OrdinalIgnoreCase)
            ? throw new InputCancelledException()
            : trimmed;
    }

    /// <summary>Asks until <paramref name="parse"/> accepts the answer, explaining each rejection.</summary>
    public T Read<T>(string prompt, Func<string, ParseOutcome<T>> parse)
    {
        ArgumentNullException.ThrowIfNull(parse);
        while (true)
        {
            var outcome = parse(ReadLine(prompt));
            if (outcome.IsValid)
            {
                return outcome.Value!;
            }

            _output.WriteLine($"  ! {outcome.Error}");
        }
    }

    public string ReadText(string prompt, int maxLength) =>
        Read(prompt, input => Parsers.Text(input, maxLength));

    public int ReadWholeNumber(string prompt, int min, int max) =>
        Read($"{prompt} ({min}-{max})", input => Parsers.WholeNumber(input, min, max));

    public decimal ReadAmount(string prompt, decimal max) =>
        Read(prompt, input => Parsers.Amount(input, max));

    public Isbn ReadIsbn(string prompt) => Read(prompt, Parsers.Isbn);

    /// <summary>Lists the enum's values as numbered options and asks for one.</summary>
    public TEnum ReadChoice<TEnum>(string prompt)
        where TEnum : struct, Enum
    {
        var options = Enum.GetValues<TEnum>();
        var labels = options.Select((option, index) => $"{index + 1}) {option}");
        _output.WriteLine($"  {string.Join("   ", labels)}");
        return Read(prompt, Parsers.Choice<TEnum>);
    }
}
