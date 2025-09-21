using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.RemoveItem;

public class RemoveItemRequestValidator : AbstractValidator<RemoveItemRequest>
{
    public RemoveItemRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

