namespace LibraryManagement.ConsoleApp.Tests;

/// <summary>A clock frozen at one instant in UTC, so console output is deterministic.</summary>
internal sealed class FixedClock(DateTimeOffset now) : TimeProvider
{
    public static readonly DateTimeOffset DefaultStart = new(2026, 11, 2, 9, 0, 0, TimeSpan.Zero);

    public FixedClock()
        : this(DefaultStart)
    {
    }

    public override DateTimeOffset GetUtcNow() => now;

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;
}
