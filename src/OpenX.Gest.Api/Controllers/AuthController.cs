using Microsoft.AspNetCore.Mvc;
using OpenX.Gest.Application.Features.Auth.Commands;
using OpenX.Gest.Application.Features.Auth.DTOs;

namespace OpenX.Gest.Api.Controllers;

[Route("api/v1/auth")]
public class AuthController : ApiControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
