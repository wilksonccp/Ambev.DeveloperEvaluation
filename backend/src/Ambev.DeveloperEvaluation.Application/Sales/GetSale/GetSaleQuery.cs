using MediatR;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public record GetSaleQuery(Guid Id) : IRequest<CreateSaleResult>;

