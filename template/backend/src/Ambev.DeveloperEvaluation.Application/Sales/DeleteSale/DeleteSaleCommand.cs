using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public record DeleteSaleCommand(Guid Id) : IRequest<DeleteSaleResponse>;

public class DeleteSaleResponse
{
    public bool Success { get; set; }
}
