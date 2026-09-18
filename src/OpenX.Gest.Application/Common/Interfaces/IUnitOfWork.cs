namespace OpenX.Gest.Application.Common.Interfaces;

/// <summary>
/// Unit of Work contract ensuring transactional atomicity across repository mutations.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
