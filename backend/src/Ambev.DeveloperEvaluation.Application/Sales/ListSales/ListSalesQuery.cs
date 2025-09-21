using MediatR;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public record ListSalesQuery(
    int Page = 1,
    int Size = 10,
    string? Order = null,
    Guid? CustomerId = null,
    Guid? BranchId = null,
    string? Number = null
) : IRequest<IReadOnlyList<CreateSaleResult>>;

