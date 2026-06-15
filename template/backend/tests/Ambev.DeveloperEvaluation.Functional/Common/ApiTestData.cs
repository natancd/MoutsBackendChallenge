using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Functional.Common;

public static class ApiTestJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
}

public static class ApiTestData
{
    public const string ValidPassword = "NatanNatan1!";

    public static CreateUserRequest ValidUserRequest(string? suffix = null)
    {
        suffix ??= Guid.NewGuid().ToString("N")[..8];

        return new CreateUserRequest
        {
            Username = $"natan-{suffix}",
            Password = ValidPassword,
            Phone = "+5511999999999",
            Email = $"natan-{suffix}@natan.com",
            Status = UserStatus.Active,
            Role = UserRole.Customer
        };
    }

    public static AuthenticateUserRequest ValidAuthRequest(CreateUserRequest user) => new()
    {
        Email = user.Email,
        Password = user.Password
    };

    public static CreateSaleRequest ValidSaleRequest(string? saleNumber = null) => new()
    {
        SaleNumber = saleNumber ?? $"SALE-{Guid.NewGuid():N}"[..14],
        SaleDate = DateTime.UtcNow,
        Customer = new ExternalIdentityRequest
        {
            Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            Name = "Cliente ABC"
        },
        Branch = new ExternalIdentityRequest
        {
            Id = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
            Name = "Filial Centro"
        },
        Items =
        [
            new SaleItemRequest
            {
                ProductId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                ProductName = "Produto A",
                Quantity = 10,
                UnitPrice = 5.50m
            }
        ]
    };
}

public static class HttpTestExtensions
{
    public static async Task<T?> ReadApiDataAsync<T>(this HttpResponseMessage response) where T : class
    {
        var body = await response.Content.ReadAsStringAsync();
        var wrapper = JsonSerializer.Deserialize<ApiResponseWithData<T>>(body, ApiTestJson.Options);
        return wrapper?.Data;
    }

    public static async Task<ApiResponseWithData<T>?> ReadApiResponseAsync<T>(this HttpResponseMessage response) where T : class
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponseWithData<T>>(body, ApiTestJson.Options);
    }

    public static async Task AssertApiErrorAsync(
        this HttpResponseMessage response,
        int expectedStatusCode,
        string? expectedType = null)
    {
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be((System.Net.HttpStatusCode)expectedStatusCode,
            $"response body: {body}");

        if (expectedType is not null)
            body.Should().Contain(expectedType);
    }
}
