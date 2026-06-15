using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleResultDto>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly IMapper _mapper;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        ISaleEventPublisher eventPublisher,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _eventPublisher = eventPublisher;
        _mapper = mapper;
    }

    public async Task<SaleResultDto> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var existingSale = await _saleRepository.GetBySaleNumberAsync(command.SaleNumber, cancellationToken);
        if (existingSale != null)
            throw new InvalidOperationException(
                $"A sale with number '{command.SaleNumber}' already exists (ID {existingSale.Id}).");

        var customer = _mapper.Map<ExternalIdentity>(command.Customer);
        var branch = _mapper.Map<ExternalIdentity>(command.Branch);
        var items = command.Items.Select(i => (i.ProductId, i.ProductName, i.Quantity, i.UnitPrice));

        var sale = Sale.Create(command.SaleNumber, command.SaleDate, customer, branch, items);
        var createdSale = await _saleRepository.CreateAsync(sale, cancellationToken);

        await _eventPublisher.PublishSaleCreatedAsync(new SaleCreatedEvent(createdSale), cancellationToken);

        return _mapper.Map<SaleResultDto>(createdSale);
    }
}
