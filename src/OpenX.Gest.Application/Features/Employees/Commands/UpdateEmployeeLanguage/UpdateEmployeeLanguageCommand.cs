using MediatR;
using OpenX.Gest.Domain.Common;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeLanguage;

/// <summary>
/// Command to update the preferred UI and compliance language of an employee.
/// </summary>
public record UpdateEmployeeLanguageCommand(Guid EmployeeId, LanguageCode Language) : IRequest<Result>;
