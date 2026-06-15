using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleCommand : IRequest<SaleResultDto>
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public ExternalIdentityCommandDto Customer { get; set; } = new();
    public ExternalIdentityCommandDto Branch { get; set; } = new();
    public List<SaleItemCommandDto> Items { get; set; } = [];
}
