using Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleRequestValidator : AbstractValidator<UpdateSaleRequest>
{
    public UpdateSaleRequestValidator()
    {
        RuleFor(x => x.SaleDate).NotEmpty();
        RuleFor(x => x.Customer.Id).NotEmpty();
        RuleFor(x => x.Customer.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Branch.Id).NotEmpty();
        RuleFor(x => x.Branch.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x).Must(x => x.Customer.Id != x.Branch.Id)
            .WithMessage("Customer and branch must be different external identities.");
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new CreateSale.SaleItemRequestValidator());
    }
}
