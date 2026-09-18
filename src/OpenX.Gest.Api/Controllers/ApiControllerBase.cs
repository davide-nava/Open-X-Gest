using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return CreateProblemDetails(result.Error, result.Errors);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return CreateProblemDetails(result.Error, result.Errors);
    }

    private IActionResult CreateProblemDetails(Error primaryError, IReadOnlyList<Error> errors)
    {
        var statusCode = primaryError.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.LegalViolation => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status400BadRequest
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = primaryError.Code,
            Detail = primaryError.Description,
            Instance = HttpContext.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        if (errors.Count > 1)
        {
            problemDetails.Extensions["errors"] = errors.Select(e => new
            {
                code = e.Code,
                description = e.Description,
                type = e.Type.ToString()
            });
        }

        return StatusCode(statusCode, problemDetails);
    }
}
