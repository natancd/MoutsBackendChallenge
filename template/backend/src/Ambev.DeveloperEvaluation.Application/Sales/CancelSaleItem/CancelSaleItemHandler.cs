using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, SaleResultDto>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public CancelSaleItemHandler(
        ISaleRepository saleRepository,
        ISaleEventPublisher eventPublisher,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<SaleResultDto> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleItemValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken) ?? throw new KeyNotFoundException($"The sale with ID '{request.SaleId}' was not found.");
        var item = sale.Items.FirstOrDefault(i => i.Id == request.ItemId) ?? throw new KeyNotFoundException(
                $"The sale item with ID '{request.ItemId}' was not found in sale '{sale.SaleNumber}' (ID {sale.Id}).");
        sale.CancelItem(request.ItemId);
        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);

        var cancelledItem = updatedSale.Items.First(i => i.Id == request.ItemId);
        await _eventPublisher.PublishItemCancelledAsync(new ItemCancelledEvent(updatedSale, cancelledItem), cancellationToken);

        return _mapper.Map<SaleResultDto>(updatedSale);
    }
}
