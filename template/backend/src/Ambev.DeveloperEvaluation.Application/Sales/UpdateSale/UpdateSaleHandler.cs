using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, SaleResultDto>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public UpdateSaleHandler(
        ISaleRepository saleRepository,
        ISaleEventPublisher eventPublisher,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<SaleResultDto> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsNoTrackingAsync(command.Id, cancellationToken) ?? throw new KeyNotFoundException($"The sale with ID '{command.Id}' was not found.");
        var customer = _mapper.Map<ExternalIdentity>(command.Customer);
        var branch = _mapper.Map<ExternalIdentity>(command.Branch);
        var items = command.Items.Select(i => (i.ProductId, i.ProductName, i.Quantity, i.UnitPrice));

        sale.Update(command.SaleDate, customer, branch, items);
        var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);

        await _eventPublisher.PublishSaleModifiedAsync(new SaleModifiedEvent(updatedSale), cancellationToken);

        return _mapper.Map<SaleResultDto>(updatedSale);
    }
}
