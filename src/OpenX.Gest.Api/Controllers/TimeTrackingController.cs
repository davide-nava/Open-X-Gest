using Microsoft.AspNetCore.Mvc;
using OpenX.Gest.Application.Features.TimeTracking.Commands.ClockIn;
using OpenX.Gest.Application.Features.TimeTracking.Commands.ClockOut;
using OpenX.Gest.Application.Features.TimeTracking.Commands.CorrectTimeEntry;
using OpenX.Gest.Application.Features.TimeTracking.DTOs;
using OpenX.Gest.Application.Features.TimeTracking.Queries.ExportSecoReport;
using OpenX.Gest.Application.Features.TimeTracking.Queries.GetCurrentStatus;
using OpenX.Gest.Application.Features.TimeTracking.Queries.GetTimesheet;

namespace OpenX.Gest.Api.Controllers;

[Route("api/v1/timetracking")]
public class TimeTrackingController : ApiControllerBase
{
    /// <summary>
    /// Registra la timbratura di entrata con cattura facoltativa di coordinate GPS puntuali (Art. 26 OLL 3).
    /// </summary>
    [HttpPost("clock-in")]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ClockIn([FromBody] ClockInCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Registra la timbratura di uscita, le pause effettuate e valuta le violazioni legali (Art. 10, 15, 15a LL).
    /// </summary>
    [HttpPost("clock-out")]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClockOut([FromBody] ClockOutCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Rettifica retroattiva di una timbratura con causale legale obbligatoria (Art. 73 OLL 1).
    /// </summary>
    [HttpPost("correct")]
    [ProducesResponseType(typeof(TimeEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Correct([FromBody] CorrectTimeEntryCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Recupera lo stato attuale della presenza del collaboratore in tempo reale.
    /// </summary>
    [HttpGet("status/{employeeId:guid}")]
    [ProducesResponseType(typeof(CurrentTimeStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid employeeId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCurrentStatusQuery(employeeId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Recupera il cartellino presenze (timesheet) per un intervallo di date, con ripartizione oraria e anomalie.
    /// </summary>
    [HttpGet("timesheet")]
    [ProducesResponseType(typeof(TimesheetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTimesheet(
        [FromQuery] Guid employeeId,
        [FromQuery] DateTime startDateUtc,
        [FromQuery] DateTime endDateUtc,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTimesheetQuery(employeeId, startDateUtc, endDateUtc), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Esporta il report di conformità SECO per l'ispettorato del lavoro in formato CSV o PDF (Art. 73 OLL 1).
    /// </summary>
    [HttpGet("export/seco")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportSeco(
        [FromQuery] Guid employeeId,
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] string format = "csv",
        [FromQuery] string language = "it",
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new ExportSecoReportQuery(employeeId, year, month, format, language), cancellationToken);
        if (result.IsFailure)
        {
            return HandleResult(result);
        }

        return File(result.Value.FileBytes, result.Value.ContentType, result.Value.FileName);
    }
}
