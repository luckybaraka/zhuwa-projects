using LibraryManagement.ConsoleApp.Time;

namespace LibraryManagement.ConsoleApp.Tests.Time;

public class AdjustableClockTests
{
    private static readonly DateTimeOffset Start = FixedClock.DefaultStart;

    [Fact]
    public void StartsAtTheBaseClockTime()
    {
        var clock = new AdjustableClock(new FixedClock(Start));

        Assert.Equal(Start, clock.GetUtcNow());
        Assert.Equal(0, clock.DaysAdvanced);
        Assert.Equal(TimeZoneInfo.Utc, clock.LocalTimeZone);
    }

    [Fact]
    public void AdvanceDays_Accumulates()
    {
        var clock = new AdjustableClock(new FixedClock(Start));

        clock.AdvanceDays(10);
        clock.AdvanceDays(5);

        Assert.Equal(Start.AddDays(15), clock.GetUtcNow());
        Assert.Equal(15, clock.DaysAdvanced);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(AdjustableClock.MaxAdvanceDays + 1)]
    public void AdvanceDays_OutOfRange_Throws(int days)
    {
        var clock = new AdjustableClock(new FixedClock(Start));

        Assert.Throws<ArgumentOutOfRangeException>(() => clock.AdvanceDays(days));
    }
}
