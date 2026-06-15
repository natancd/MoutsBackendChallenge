using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Services;
using AutoMapper;
using Bogus;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleEventPublisher _eventPublisher;
    private readonly IMapper _mapper;
    private readonly CreateSaleHandler _handler;
    private readonly Faker _faker;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _eventPublisher = Substitute.For<ISaleEventPublisher>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateSaleHandler(_saleRepository, _eventPublisher, _mapper);
        _faker = new Faker();
    }

    [Fact(DisplayName = "Given valid sale data When creating sale Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        var command = CreateValidCommand();
        var sale = new Sale { Id = Guid.NewGuid(), SaleNumber = command.SaleNumber, TotalAmount = 100m };
        var result = new SaleResultDto { Id = sale.Id, SaleNumber = sale.SaleNumber };

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _mapper.Map<ExternalIdentity>(command.Customer)
            .Returns(new ExternalIdentity(command.Customer.Id, command.Customer.Name));
        _mapper.Map<ExternalIdentity>(command.Branch)
            .Returns(new ExternalIdentity(command.Branch.Id, command.Branch.Name));
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(sale);
        _mapper.Map<SaleResultDto>(sale).Returns(result);

        var createSaleResult = await _handler.Handle(command, CancellationToken.None);

        createSaleResult.Should().NotBeNull();
        createSaleResult.SaleNumber.Should().Be(command.SaleNumber);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _eventPublisher.Received(1).PublishSaleCreatedAsync(Arg.Any<SaleCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid sale data When creating sale Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var command = new CreateSaleCommand();

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    private CreateSaleCommand CreateValidCommand()
    {
        return new CreateSaleCommand
        {
            SaleNumber = $"SALE-{_faker.Random.AlphaNumeric(6)}",
            SaleDate = DateTime.UtcNow,
            Customer = new ExternalIdentityCommandDto
            {
                Id = Guid.NewGuid(),
                Name = _faker.Person.FullName
            },
            Branch = new ExternalIdentityCommandDto
            {
                Id = Guid.NewGuid(),
                Name = _faker.Company.CompanyName()
            },
            Items = new List<SaleItemCommandDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = _faker.Commerce.ProductName(),
                    Quantity = 4,
                    UnitPrice = 25m
                }
            }
        };
    }
}
