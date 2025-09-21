using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.RemoveItem;

public class RemoveItemFromSaleCommandValidator : AbstractValidator<RemoveItemFromSaleCommand>
{
    public RemoveItemFromSaleCommandValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

