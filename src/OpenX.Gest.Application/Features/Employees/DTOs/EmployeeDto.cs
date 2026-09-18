using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;

namespace OpenX.Gest.Application.Features.Employees.DTOs;

public record EmployeeDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Department,
    decimal ContractualWeeklyHours,
    StatutoryWeeklyLimit StatutoryWeeklyLimit,
    Oll1Regime Oll1Regime,
    LanguageCode PreferredLanguage,
    bool IsActive
)
{
    public static EmployeeDto FromEntity(Employee employee) => new(
        employee.Id,
        employee.FirstName,
        employee.LastName,
        employee.Email,
        employee.Department,
        employee.ContractualWeeklyHours,
        employee.StatutoryWeeklyLimit,
        employee.Oll1Regime,
        employee.PreferredLanguage,
        employee.IsActive
    );
}
