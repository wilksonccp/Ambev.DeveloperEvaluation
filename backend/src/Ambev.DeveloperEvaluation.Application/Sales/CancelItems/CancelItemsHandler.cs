using MediatR;
using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelItems;

public class CancelItemsHandler : IRequestHandler<CancelItemsCommand, CreateSale.CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IDomainEventPublisher _eventPublisher;

    public CancelItemsHandler(ISaleRepository saleRepository, IMapper mapper, IDomainEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<CreateSale.CreateSaleResult> Handle(CancelItemsCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");

        var productIdsToCancel = sale.ReadOnlyItems.Where(i => !i.IsCancelled).Select(i => i.ProductId).ToList();
        sale.CancelItems();
        await _saleRepository.UpdateAsync(sale, cancellationToken);
        if (productIdsToCancel.Count > 0)
        {
            await _eventPublisher.PublishAsync(new ItemCancelledDomainEvent(sale.Id, productIdsToCancel), cancellationToken);
        }
        return _mapper.Map<CreateSale.CreateSaleResult>(sale);
    }
}
