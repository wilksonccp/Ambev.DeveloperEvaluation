using AutoMapper;
using MediatR;
using FluentValidation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IDomainEventPublisher _eventPublisher;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, IDomainEventPublisher eventPublisher)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var items = request.Items?.Select(i => (i.ProductId, i.ProductName, i.UnitPrice, i.Quantity));

        var sale = Sale.CreateNew(
            request.Id,
            request.Number,
            request.CustomerId,
            request.CustomerName,
            request.BranchId,
            request.BranchName,
            items);

        await _saleRepository.AddAsync(sale, cancellationToken);

        await _eventPublisher.PublishAsync(new SaleCreatedDomainEvent(
            sale.Id,
            sale.Number,
            sale.CustomerId,
            sale.BranchId,
            sale.TotalAmount,
            sale.TotalDiscount,
            sale.TotalPayable), cancellationToken);

        var result = _mapper.Map<CreateSaleResult>(sale);
        return result;
    }
}
