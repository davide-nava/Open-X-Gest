using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.Features.Auth.DTOs;

/// <summary>
/// Authentication response containing the JWT bearer token and employee profile details.
/// </summary>
public record AuthResponseDto(
    string Token,
    Guid EmployeeId,
    string FullName,
    string Email,
    string Role,
    LanguageCode PreferredLanguage,
    Oll1Regime Oll1Regime
);
