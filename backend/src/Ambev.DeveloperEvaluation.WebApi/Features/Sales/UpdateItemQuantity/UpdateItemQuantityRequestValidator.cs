using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateItemQuantity;

public class UpdateItemQuantityRequestValidator : AbstractValidator<UpdateItemQuantityRequest>
{
    public UpdateItemQuantityRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

