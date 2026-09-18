using MediatR;
using Microsoft.Extensions.Localization;
using OpenX.Gest.Application.Common.Interfaces;
using OpenX.Gest.Application.Resources;
using OpenX.Gest.Domain.Common;

namespace OpenX.Gest.Application.Features.Employees.Commands.UpdateEmployeeRegime;

/// <summary>
/// MediatR request handler for updating an employee's OLL 1 working time regime.
/// </summary>
public class UpdateEmployeeRegimeCommandHandler : IRequestHandler<UpdateEmployeeRegimeCommand, Result>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<ValidationMessages> _localizer;

    public UpdateEmployeeRegimeCommandHandler(
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork,
        IStringLocalizer<ValidationMessages> localizer)
    {
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task<Result> Handle(UpdateEmployeeRegimeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return Result.Failure(Error.NotFound("Employee.NotFound", _localizer["EmployeeNotFound"]));
        }

        employee.UpdateOll1Regime(request.Regime);
        _employeeRepository.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
