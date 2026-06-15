using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales;

public class LoggingSaleEventPublisher : ISaleEventPublisher
{
    private readonly ILogger<LoggingSaleEventPublisher> _logger;

    public LoggingSaleEventPublisher(ILogger<LoggingSaleEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishSaleCreatedAsync(SaleCreatedEvent saleEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "SaleCreated: SaleId={SaleId}, SaleNumber={SaleNumber}, TotalAmount={TotalAmount}",
            saleEvent.Sale.Id,
            saleEvent.Sale.SaleNumber,
            saleEvent.Sale.TotalAmount);

        return Task.CompletedTask;
    }

    public Task PublishSaleModifiedAsync(SaleModifiedEvent saleEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "SaleModified: SaleId={SaleId}, SaleNumber={SaleNumber}, TotalAmount={TotalAmount}",
            saleEvent.Sale.Id,
            saleEvent.Sale.SaleNumber,
            saleEvent.Sale.TotalAmount);

        return Task.CompletedTask;
    }

    public Task PublishSaleCancelledAsync(SaleCancelledEvent saleEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "SaleCancelled: SaleId={SaleId}, SaleNumber={SaleNumber}",
            saleEvent.Sale.Id,
            saleEvent.Sale.SaleNumber);

        return Task.CompletedTask;
    }

    public Task PublishItemCancelledAsync(ItemCancelledEvent itemEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "ItemCancelled: SaleId={SaleId}, ItemId={ItemId}, ProductName={ProductName}",
            itemEvent.Sale.Id,
            itemEvent.Item.Id,
            itemEvent.Item.ProductName);

        return Task.CompletedTask;
    }
}
