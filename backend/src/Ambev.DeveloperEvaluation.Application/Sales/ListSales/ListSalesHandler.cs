using MediatR;
using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesQuery, IReadOnlyList<CreateSale.CreateSaleResult>>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CreateSale.CreateSaleResult>> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        var sales = await _saleRepository.ListAsync(
            request.Page,
            request.Size,
            request.Order,
            request.CustomerId,
            request.BranchId,
            request.Number,
            cancellationToken);

        return sales.Select(s => _mapper.Map<CreateSale.CreateSaleResult>(s)).ToList();
    }
}
