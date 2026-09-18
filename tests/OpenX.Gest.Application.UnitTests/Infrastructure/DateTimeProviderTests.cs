using OpenX.Gest.Infrastructure.Services;

namespace OpenX.Gest.Application.UnitTests.Infrastructure;

public class DateTimeProviderTests
{
    private readonly DateTimeProvider _provider = new();

    [Fact]
    public void UtcNow_ShouldReturnCurrentUtcTime()
    {
        var before = DateTime.UtcNow;
        var now = _provider.UtcNow;
        var after = DateTime.UtcNow;

        now.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void SwissNow_ShouldHaveOffsetRelativeToUtc()
    {
        var utc = _provider.UtcNow;
        var swiss = _provider.SwissNow;

        // Switzerland is either UTC+1 (standard) or UTC+2 (daylight saving)
        var diffHours = (swiss - utc).TotalHours;
        diffHours.Should().BeInRange(0.9, 2.1);
    }

    [Fact]
    public void ToSwissTime_InWinter_ShouldAddOneHour()
    {
        // 15 January: Standard time UTC+1
        var winterUtc = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var swissTime = _provider.ToSwissTime(winterUtc);

        swissTime.Hour.Should().Be(11);
    }

    [Fact]
    public void ToSwissTime_InSummer_ShouldAddTwoHours()
    {
        // 15 July: Daylight saving time UTC+2
        var summerUtc = new DateTime(2026, 7, 15, 10, 0, 0, DateTimeKind.Utc);
        var swissTime = _provider.ToSwissTime(summerUtc);

        swissTime.Hour.Should().Be(12);
    }

    [Fact]
    public void ToUtc_ShouldReverseConversionProperly()
    {
        var winterUtc = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var swissTime = _provider.ToSwissTime(winterUtc);
        var backToUtc = _provider.ToUtc(swissTime);

        backToUtc.Should().Be(winterUtc);
    }
}
