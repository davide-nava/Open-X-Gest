using MediatR;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Resources;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeLanguage;

/// <summary>
/// MediatR request handler for updating an employee's preferred language.
/// </summary>
public class UpdateEmployeeLanguageCommandHandler : IRequestHandler<UpdateEmployeeLanguageCommand, Result>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<ValidationMessages> _localizer;

    public UpdateEmployeeLanguageCommandHandler(
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork,
        IStringLocalizer<ValidationMessages> localizer)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task<Result> Handle(UpdateEmployeeLanguageCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return Result.Failure(Error.NotFound("Employee.NotFound", _localizer["EmployeeNotFound"]));
        }

        employee.SetPreferredLanguage(request.Language);
        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
