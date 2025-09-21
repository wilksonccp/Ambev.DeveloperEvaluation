using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.AddItem;

public class AddItemToSaleCommandValidator : AbstractValidator<AddItemToSaleCommand>
{
    public AddItemToSaleCommandValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty();
        RuleFor(x => x.UnitPrice).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

