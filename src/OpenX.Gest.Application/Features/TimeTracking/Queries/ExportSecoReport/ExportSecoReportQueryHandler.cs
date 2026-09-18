using MediatR;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Application.Resources;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.TimeTracking.Queries.ExportSecoReport;

/// <summary>
/// MediatR request handler for generating statutory SECO compliance export reports.
/// </summary>
public class ExportSecoReportQueryHandler : IRequestHandler<ExportSecoReportQuery, Result<SecoExportDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ISecoComplianceExportService _exportService;
    private readonly IStringLocalizer<ValidationMessages> _localizer;

    public ExportSecoReportQueryHandler(
        IEmployeeRepository employeeRepository,
        ITimeEntryRepository timeEntryRepository,
        ISecoComplianceExportService exportService,
        IStringLocalizer<ValidationMessages> localizer)
    {
        _employeeRepository = employeeRepository;
        _timeEntryRepository = timeEntryRepository;
        _exportService = exportService;
        _localizer = localizer;
    }

    public async Task<Result<SecoExportDto>> Handle(ExportSecoReportQuery request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return Result<SecoExportDto>.Failure(Error.NotFound("Employee.NotFound", _localizer["EmployeeNotFound"]));
        }

        var startUtc = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endUtc = startUtc.AddMonths(1).AddSeconds(-1);

        var entries = await _timeEntryRepository.GetEntriesForEmployeeRangeAsync(
            request.EmployeeId,
            startUtc,
            endUtc,
            cancellationToken);

        byte[] bytes;
        string contentType;
        string fileName;

        if (string.Equals(request.Format, "pdf", StringComparison.OrdinalIgnoreCase))
        {
            bytes = await _exportService.GenerateInspectionSummaryPdfAsync(employee, entries, startUtc, endUtc, request.Language, cancellationToken);
            contentType = "application/pdf";
            fileName = $"SECO_Report_{employee.LastName}_{request.Year}_{request.Month:D2}.pdf";
        }
        else
        {
            bytes = await _exportService.GenerateCsvReportAsync(employee, entries, startUtc, endUtc, request.Language, cancellationToken);
            contentType = "text/csv";
            fileName = $"SECO_Report_{employee.LastName}_{request.Year}_{request.Month:D2}.csv";
        }

        return Result<SecoExportDto>.Success(new SecoExportDto(fileName, contentType, bytes));
    }
}
