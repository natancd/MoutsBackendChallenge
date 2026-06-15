using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Domain.Services;

public interface ISaleEventPublisher
{
    Task PublishSaleCreatedAsync(SaleCreatedEvent saleEvent, CancellationToken cancellationToken = default);
    Task PublishSaleModifiedAsync(SaleModifiedEvent saleEvent, CancellationToken cancellationToken = default);
    Task PublishSaleCancelledAsync(SaleCancelledEvent saleEvent, CancellationToken cancellationToken = default);
    Task PublishItemCancelledAsync(ItemCancelledEvent itemEvent, CancellationToken cancellationToken = default);
}
