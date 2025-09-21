using AutoMapper;
using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleProfile : Profile
{
    public CreateSaleProfile()
    {
        // Domain -> Result only (command mapping will be handled in handler)
        CreateMap<SaleItem, CreateSaleItemResult>();
        CreateMap<Sale, CreateSaleResult>()
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.ReadOnlyItems));
    }
}
