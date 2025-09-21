using MediatR;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelItems;

public record CancelItemsCommand(Guid SaleId) : IRequest<CreateSaleResult>;

