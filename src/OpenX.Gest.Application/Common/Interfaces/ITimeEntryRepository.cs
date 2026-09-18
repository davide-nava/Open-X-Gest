using OpenX.Gest.Domain.Entities;

namespace OpenX.Gest.Application.Common.Interfaces;

/// <summary>
/// Repository abstraction for managing TimeEntry aggregates.
/// </summary>
public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TimeEntry?> GetActiveEntryForEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<TimeEntry?> GetPreviousEntryBeforeAsync(Guid employeeId, DateTime utcTimestamp, CancellationToken cancellationToken = default);
    Task<List<TimeEntry>> GetEntriesForEmployeeRangeAsync(Guid employeeId, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);
    Task AddAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default);
    Task AddCorrectionAuditAsync(TimeCorrectionAudit audit, CancellationToken cancellationToken = default);
    void Update(TimeEntry timeEntry);
}
