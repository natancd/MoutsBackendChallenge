using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Functional.Common;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Functional.Sales;

public class SalesApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SalesApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/Sales - 201 Created com venda válida e desconto de 20%")]
    public async Task CreateSale_ValidRequest_ReturnsCreatedWithDiscount()
    {
        var response = await _client.PostAsJsonAsync("/api/Sales", ApiTestData.ValidSaleRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadApiResponseAsync<SaleResponse>();
        body!.Success.Should().BeTrue();
        body.Data!.TotalAmount.Should().Be(44.00m);
        body.Data.Items.Should().ContainSingle();
        body.Data.Items[0].DiscountPercentage.Should().Be(20m);
    }

    [Fact(DisplayName = "POST /api/Sales - 400 ValidationError com mais de 20 itens iguais")]
    public async Task CreateSale_MoreThanTwentyItems_ReturnsBadRequest()
    {
        var request = ApiTestData.ValidSaleRequest();
        request.Items[0].Quantity = 21;

        var response = await _client.PostAsJsonAsync("/api/Sales", request);

        await response.AssertApiErrorAsync((int)HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST /api/Sales - 400 ValidationError com payload vazio")]
    public async Task CreateSale_InvalidRequest_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/Sales", new CreateSaleRequest());

        await response.AssertApiErrorAsync((int)HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "GET /api/Sales/{id} - 200 OK quando venda existe")]
    public async Task GetSale_ExistingSale_ReturnsOk()
    {
        var created = await CreateSaleAsync();

        var response = await _client.GetAsync($"/api/Sales/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadApiResponseAsync<SaleResponse>();
        body!.Data!.Id.Should().Be(created.Id);
    }

    [Fact(DisplayName = "GET /api/Sales/{id} - 404 ResourceNotFound quando venda não existe")]
    public async Task GetSale_NotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/Sales/{Guid.NewGuid()}");

        await response.AssertApiErrorAsync((int)HttpStatusCode.NotFound, "ResourceNotFound");
    }

    [Fact(DisplayName = "GET /api/Sales - 200 OK com paginação")]
    public async Task ListSales_ReturnsPaginatedResult()
    {
        await CreateSaleAsync();
        await CreateSaleAsync();

        var response = await _client.GetAsync("/api/Sales?_page=1&_size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = JsonSerializer.Deserialize<PaginatedResponse<SaleResponse>>(
            await response.Content.ReadAsStringAsync(),
            ApiTestJson.Options);

        body!.Success.Should().BeTrue();
        body.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact(DisplayName = "PUT /api/Sales/{id} - 200 OK ao atualizar venda")]
    public async Task UpdateSale_ExistingSale_ReturnsOk()
    {
        var created = await CreateSaleAsync();

        var updateRequest = new UpdateSaleRequest
        {
            SaleDate = DateTime.UtcNow,
            Customer = new ExternalIdentityRequest
            {
                Id = created.CustomerId,
                Name = created.CustomerName
            },
            Branch = new ExternalIdentityRequest
            {
                Id = created.BranchId,
                Name = created.BranchName
            },
            Items =
            [
                new SaleItemRequest
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Produto B",
                    Quantity = 4,
                    UnitPrice = 10m
                }
            ]
        };

        var response = await _client.PutAsJsonAsync($"/api/Sales/{created.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadApiResponseAsync<SaleResponse>();
        body!.Data!.TotalAmount.Should().Be(36.00m);
    }

    [Fact(DisplayName = "PATCH /api/Sales/{id}/cancel - 200 OK ao cancelar venda")]
    public async Task CancelSale_ExistingSale_ReturnsCancelled()
    {
        var created = await CreateSaleAsync();

        var response = await _client.PatchAsync($"/api/Sales/{created.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadApiResponseAsync<SaleResponse>();
        body!.Data!.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "PATCH /api/Sales/{saleId}/items/{itemId}/cancel - 200 OK ao cancelar item")]
    public async Task CancelSaleItem_ExistingItem_RecalculatesTotal()
    {
        var created = await CreateSaleAsync();
        var itemId = created.Items[0].Id;
        var originalTotal = created.TotalAmount;

        var response = await _client.PatchAsync($"/api/Sales/{created.Id}/items/{itemId}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadApiResponseAsync<SaleResponse>();
        body!.Data!.Items[0].IsCancelled.Should().BeTrue();
        body.Data.TotalAmount.Should().BeLessThan(originalTotal);
    }

    [Fact(DisplayName = "DELETE /api/Sales/{id} - 200 OK ao excluir venda")]
    public async Task DeleteSale_ExistingSale_ReturnsOk()
    {
        var created = await CreateSaleAsync();

        var response = await _client.DeleteAsync($"/api/Sales/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/Sales/{created.Id}");
        await getResponse.AssertApiErrorAsync((int)HttpStatusCode.NotFound, "ResourceNotFound");
    }

    private async Task<SaleResponse> CreateSaleAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/Sales", ApiTestData.ValidSaleRequest());
        response.EnsureSuccessStatusCode();
        return (await response.ReadApiDataAsync<SaleResponse>())!;
    }
}
