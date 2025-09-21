using MediatR;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

namespace Ambev.DeveloperEvaluation.Application.Sales.RemoveItem;

public record RemoveItemFromSaleCommand(
    Guid SaleId,
    Guid ProductId,
    int Quantity
) : IRequest<CreateSaleResult>;

