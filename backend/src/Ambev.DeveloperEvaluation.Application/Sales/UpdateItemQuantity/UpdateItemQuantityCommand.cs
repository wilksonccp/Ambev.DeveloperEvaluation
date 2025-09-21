using MediatR;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateItemQuantity;

public record UpdateItemQuantityCommand(
    Guid SaleId,
    Guid ProductId,
    int Quantity
) : IRequest<CreateSaleResult>;

