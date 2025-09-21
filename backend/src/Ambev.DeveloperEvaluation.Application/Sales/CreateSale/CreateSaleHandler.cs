using AutoMapper;
using MediatR;
using FluentValidation;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
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

        var result = _mapper.Map<CreateSaleResult>(sale);
        return result;
    }
}
