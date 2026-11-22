using System.Globalization;

namespace LibraryManagement.Core.Common;

/// <summary>Formats amounts the same way everywhere (fines are in Kenyan shillings).</summary>
public static class Money
{
    public const string Currency = "KES";

    public static string Format(decimal amount) =>
        string.Create(CultureInfo.InvariantCulture, $"{Currency} {amount:N2}");
}
