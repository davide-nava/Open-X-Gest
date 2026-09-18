using MediatR;
using OpenX.Gest.Application.Features.Auth.DTOs;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.Auth.Commands;

/// <summary>
/// Command to authenticate an employee with email and password credentials.
/// </summary>
public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;
