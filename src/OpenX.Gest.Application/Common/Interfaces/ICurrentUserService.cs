namespace OpenX.Gest.Application.Common.Interfaces;

/// <summary>
/// Abstraction for accessing authenticated user claims and context.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    string PreferredLanguage { get; }
}
