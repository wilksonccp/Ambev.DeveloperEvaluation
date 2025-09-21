using MediatR;
using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelItems;

public class CancelItemsHandler : IRequestHandler<CancelItemsCommand, CreateSale.CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public CancelItemsHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<CreateSale.CreateSaleResult> Handle(CancelItemsCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);
        if (sale is null)
            throw new KeyNotFoundException($"Sale with ID {request.SaleId} not found");

        sale.CancelItems();
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        return _mapper.Map<CreateSale.CreateSaleResult>(sale);
    }
}
