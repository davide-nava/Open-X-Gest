using OpenX.Gest.Domain.Entities;

namespace OpenX.Gest.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Employee employee, string role);
}
