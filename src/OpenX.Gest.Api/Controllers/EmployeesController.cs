using Microsoft.AspNetCore.Mvc;
using OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeLanguage;
using OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeRegime;
using OpenX.Gest.Application.Features.Employees.DTOs;
using OpenX.Gest.Application.Features.Employees.Queries.GetEmployeeById;
using OpenX.Gest.Application.Features.Employees.Queries.GetEmployees;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Api.Controllers;

[Route("api/v1/employees")]
public class EmployeesController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetEmployeesQuery(), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetEmployeeByIdQuery(id), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/regime")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRegime(Guid id, [FromBody] Oll1Regime regime, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateEmployeeRegimeCommand(id, regime), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id:guid}/language")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLanguage(Guid id, [FromBody] LanguageCode language, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateEmployeeLanguageCommand(id, language), cancellationToken);
        return HandleResult(result);
    }
}
