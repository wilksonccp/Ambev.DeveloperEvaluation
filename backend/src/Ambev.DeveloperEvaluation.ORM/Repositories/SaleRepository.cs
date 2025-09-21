using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sale?> GetByNumberAsync(string number, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .FirstOrDefaultAsync(s => s.Number == number, cancellationToken);
    }

    public async Task AddAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> ListAsync(
        int page,
        int size,
        string? order = null,
        Guid? customerId = null,
        Guid? branchId = null,
        string? number = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Sale> query = _context.Sales.AsNoTracking();

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);
        if (branchId.HasValue)
            query = query.Where(s => s.BranchId == branchId.Value);
        if (!string.IsNullOrWhiteSpace(number))
            query = query.Where(s => s.Number == number);

        // Simple ordering support: "number", "-number", "createdAt", "-createdAt"
        switch ((order ?? "-createdAt").Trim().ToLowerInvariant())
        {
            case "number":
                query = query.OrderBy(s => s.Number);
                break;
            case "-number":
                query = query.OrderByDescending(s => s.Number);
                break;
            case "createdat":
                query = query.OrderBy(s => s.CreatedAt);
                break;
            case "-createdat":
            default:
                query = query.OrderByDescending(s => s.CreatedAt);
                break;
        }

        if (page < 1) page = 1;
        if (size < 1) size = 10;

        query = query.Skip((page - 1) * size).Take(size);

        return await query.ToListAsync(cancellationToken);
    }
}
