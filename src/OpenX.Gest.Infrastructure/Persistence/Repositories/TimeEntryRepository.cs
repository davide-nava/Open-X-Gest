using Microsoft.EntityFrameworkCore;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Domain.Entities;

namespace OpenX.Gest.Infrastructure.Persistence.Repositories;

public class TimeEntryRepository : ITimeEntryRepository
{
    private readonly OpenXGestDbContext _context;

    public TimeEntryRepository(OpenXGestDbContext context)
    {
        _context = context;
    }

    public async Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TimeEntries
            .Include(t => t.AuditTrail)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<TimeEntry?> GetActiveEntryForEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.TimeEntries
            .Include(t => t.AuditTrail)
            .Where(t => t.EmployeeId == employeeId && t.ClockOutUtc == null)
            .OrderByDescending(t => t.ClockInUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TimeEntry?> GetPreviousEntryBeforeAsync(Guid employeeId, DateTime utcTimestamp, CancellationToken cancellationToken = default)
    {
        return await _context.TimeEntries
            .Where(t => t.EmployeeId == employeeId && t.ClockInUtc < utcTimestamp)
            .OrderByDescending(t => t.ClockInUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<TimeEntry>> GetEntriesForEmployeeRangeAsync(
        Guid employeeId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        return await _context.TimeEntries
            .Include(t => t.AuditTrail)
            .Where(t => t.EmployeeId == employeeId && t.ClockInUtc >= startUtc && t.ClockInUtc <= endUtc)
            .OrderBy(t => t.ClockInUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
    {
        await _context.TimeEntries.AddAsync(timeEntry, cancellationToken);
    }

    public async Task AddCorrectionAuditAsync(TimeCorrectionAudit audit, CancellationToken cancellationToken = default)
    {
        await _context.TimeCorrectionAudits.AddAsync(audit, cancellationToken);
    }

    public void Update(TimeEntry timeEntry)
    {
        if (_context.Entry(timeEntry).State == EntityState.Detached)
        {
            _context.TimeEntries.Attach(timeEntry);
        }
    }
}
