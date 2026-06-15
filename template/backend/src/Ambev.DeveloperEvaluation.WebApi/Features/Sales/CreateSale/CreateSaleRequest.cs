using Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

public class CreateSaleRequest
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public ExternalIdentityRequest Customer { get; set; } = new();
    public ExternalIdentityRequest Branch { get; set; } = new();
    public List<SaleItemRequest> Items { get; set; } = [];
}
