using AutoMapper;
using MediatR;
using FluentValidation;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.AddItem;

public class AddItemToSaleHandler : IRequestHandler<AddItemToSaleCommand, CreateSale.CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IDomainEventPublisher _eventPublisher;

    public AddItemToSaleHandler(ISaleRepository saleRepository, IMapper mapper, IDomainEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<CreateSale.CreateSaleResult> Handle(AddItemToSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new AddItemToSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");

        sale.AddItem(request.ProductId, request.ProductName, request.UnitPrice, request.Quantity);

        await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _eventPublisher.PublishAsync(new SaleModifiedDomainEvent(
            sale.Id,
            "ItemAdded",
            sale.TotalAmount,
            sale.TotalDiscount,
            sale.TotalPayable,
            request.ProductId,
            request.Quantity), cancellationToken);

        return _mapper.Map<CreateSale.CreateSaleResult>(sale);
    }
}
