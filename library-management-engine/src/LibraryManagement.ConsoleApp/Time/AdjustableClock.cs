namespace LibraryManagement.ConsoleApp.Time;

/// <summary>
/// The real clock plus an offset the user can move forward. It lets someone trying the
/// app see due dates, overdue items and fines without waiting two weeks.
/// The core library never knows the difference: it only sees a <see cref="TimeProvider"/>.
/// </summary>
public sealed class AdjustableClock : TimeProvider
{
    public const int MaxAdvanceDays = 365;

    private readonly TimeProvider _baseClock;
    private TimeSpan _offset;

    public AdjustableClock(TimeProvider baseClock) =>
        _baseClock = baseClock ?? throw new ArgumentNullException(nameof(baseClock));

    public override TimeZoneInfo LocalTimeZone => _baseClock.LocalTimeZone;

    /// <summary>Total days moved forward since the program started.</summary>
    public int DaysAdvanced => (int)_offset.TotalDays;

    public override DateTimeOffset GetUtcNow() => _baseClock.GetUtcNow() + _offset;

    /// <summary>Moves time forward. Time never goes backwards, so loans stay consistent.</summary>
    public void AdvanceDays(int days)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(days, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(days, MaxAdvanceDays);
        _offset += TimeSpan.FromDays(days);
    }
}
