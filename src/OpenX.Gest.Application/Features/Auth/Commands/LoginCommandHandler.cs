using MediatR;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Features.Auth.DTOs;
using OpenX.Gest.Application.Resources;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.Auth.Commands;

/// <summary>
/// MediatR request handler for processing employee login requests.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IStringLocalizer<ValidationMessages> _localizer;

    public LoginCommandHandler(
        IEmployeeRepository employeeRepository,
        IJwtTokenService jwtTokenService,
        IStringLocalizer<ValidationMessages> localizer)
    {
        _employeeRepository = employeeRepository;
        _jwtTokenService = jwtTokenService;
        _localizer = localizer;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
        if (employee == null)
        {
            return Result<AuthResponseDto>.Failure(Error.Unauthorized("Auth.InvalidCredentials", _localizer["InvalidCredentials"]));
        }

        // Determine role based on department or email identifier
        var role = employee.Email.Contains("admin") || employee.Email.Contains("hr") ? "HRManager" : "Employee";
        var token = _jwtTokenService.GenerateToken(employee, role);

        var dto = new AuthResponseDto(
            token,
            employee.Id,
            $"{employee.FirstName} {employee.LastName}",
            employee.Email,
            role,
            employee.PreferredLanguage,
            employee.Oll1Regime
        );

        return Result<AuthResponseDto>.Success(dto);
    }
}
