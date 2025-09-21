using MediatR;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

namespace Ambev.DeveloperEvaluation.Application.Sales.AddItem;

public record AddItemToSaleCommand(
    Guid SaleId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity
) : IRequest<CreateSaleResult>;

