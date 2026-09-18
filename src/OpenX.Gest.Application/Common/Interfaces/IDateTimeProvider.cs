namespace OpenX.Gest.Application.Common.Interfaces;

/// <summary>
/// Abstraction for clock operations and Swiss legal timezone conversions.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime SwissNow { get; }
    DateTime ToSwissTime(DateTime utcDateTime);
    DateTime ToUtc(DateTime swissDateTime);
}
