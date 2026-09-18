using MediatR;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.TimeTracking.Queries.ExportSecoReport;

/// <summary>
/// Query to generate SECO and Cantonal labor inspection compliance export files (CSV/PDF).
/// </summary>
public record ExportSecoReportQuery(
    Guid EmployeeId,
    int Year,
    int Month,
    string Format = "csv",
    string Language = "it"
) : IRequest<Result<SecoExportDto>>;
