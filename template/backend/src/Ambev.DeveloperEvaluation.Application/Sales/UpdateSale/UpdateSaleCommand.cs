using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleCommand : IRequest<SaleResultDto>
{
    public Guid Id { get; set; }
    public DateTime SaleDate { get; set; }
    public ExternalIdentityCommandDto Customer { get; set; } = new();
    public ExternalIdentityCommandDto Branch { get; set; } = new();
    public List<SaleItemCommandDto> Items { get; set; } = new();
}
