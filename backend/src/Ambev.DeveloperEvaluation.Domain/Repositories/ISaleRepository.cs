using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ISaleRepository
{
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sale?> GetByNumberAsync(string number, CancellationToken cancellationToken = default);
    Task AddAsync(Sale sale, CancellationToken cancellationToken = default);
    Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sale>> ListAsync(
        int page,
        int size,
        string? order = null,
        Guid? customerId = null,
        Guid? branchId = null,
        string? number = null,
        CancellationToken cancellationToken = default);
}
