using Ambev.DeveloperEvaluation.Application.Sales.Common;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemCommand : IRequest<SaleResultDto>
{
    public Guid SaleId { get; set; }
    public Guid ItemId { get; set; }
}
