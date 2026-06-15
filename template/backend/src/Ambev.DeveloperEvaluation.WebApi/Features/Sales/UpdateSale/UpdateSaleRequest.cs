using Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

public class UpdateSaleRequest
{
    public DateTime SaleDate { get; set; }
    public ExternalIdentityRequest Customer { get; set; } = new();
    public ExternalIdentityRequest Branch { get; set; } = new();
    public List<SaleItemRequest> Items { get; set; } = [];
}
