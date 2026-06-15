using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Functional.Common;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.GetUser;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Functional.Users;

public class UsersApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/Users - 201 Created com dados válidos")]
    public async Task CreateUser_ValidRequest_ReturnsCreated()
    {
        var request = ApiTestData.ValidUserRequest();
        var response = await _client.PostAsJsonAsync("/api/Users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.ReadApiResponseAsync<CreateUserResponse>();
        body!.Success.Should().BeTrue();
        body.Data!.Email.Should().Be(request.Email);
        body.Data.Name.Should().Be(request.Username);
    }

    [Fact(DisplayName = "POST /api/Users - 400 ValidationError com dados inválidos")]
    public async Task CreateUser_InvalidRequest_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/Users", new CreateUserRequest());

        await response.AssertApiErrorAsync((int)HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "GET /api/Users - 200 OK com paginação")]
    public async Task ListUsers_ReturnsPaginatedResult()
    {
        await _client.PostAsJsonAsync("/api/Users", ApiTestData.ValidUserRequest());
        await _client.PostAsJsonAsync("/api/Users", ApiTestData.ValidUserRequest());

        var response = await _client.GetAsync("/api/Users?_page=1&_size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = JsonSerializer.Deserialize<PaginatedResponse<GetUserResponse>>(
            await response.Content.ReadAsStringAsync(),
            ApiTestJson.Options);

        body!.Success.Should().BeTrue();
        body.TotalCount.Should().BeGreaterThanOrEqualTo(2);
        body.Data.Should().NotBeNull();
    }

    [Fact(DisplayName = "POST /api/Users - 400 com email duplicado (mensagem genérica)")]
    public async Task CreateUser_DuplicateEmail_ReturnsGenericError()
    {
        var request = ApiTestData.ValidUserRequest();
        await _client.PostAsJsonAsync("/api/Users", request);

        var duplicateRequest = ApiTestData.ValidUserRequest();
        duplicateRequest.Email = request.Email;

        var response = await _client.PostAsJsonAsync("/api/Users", duplicateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Invalid email address");
        body.Should().NotContain("already exists");
    }

    [Fact(DisplayName = "GET /api/Users/{id} - 200 OK quando usuário existe")]
    public async Task GetUser_ExistingUser_ReturnsOk()
    {
        var request = ApiTestData.ValidUserRequest();
        var createResponse = await _client.PostAsJsonAsync("/api/Users", request);
        var created = await createResponse.ReadApiDataAsync<CreateUserResponse>();

        var response = await _client.GetAsync($"/api/Users/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadApiResponseAsync<GetUserResponse>();
        body!.Data!.Id.Should().Be(created.Id);
        body.Data.Email.Should().Be(request.Email);
    }

    [Fact(DisplayName = "GET /api/Users/{id} - 404 ResourceNotFound quando usuário não existe")]
    public async Task GetUser_NotFound_Returns404()
    {
        var response = await _client.GetAsync($"/api/Users/{Guid.NewGuid()}");

        await response.AssertApiErrorAsync((int)HttpStatusCode.NotFound, "ResourceNotFound");
    }

    [Fact(DisplayName = "DELETE /api/Users/{id} - 200 OK quando usuário existe")]
    public async Task DeleteUser_ExistingUser_ReturnsOk()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/Users", ApiTestData.ValidUserRequest());
        var created = await createResponse.ReadApiDataAsync<CreateUserResponse>();

        var response = await _client.DeleteAsync($"/api/Users/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync($"/api/Users/{created.Id}");
        await getResponse.AssertApiErrorAsync((int)HttpStatusCode.NotFound, "ResourceNotFound");
    }

    [Fact(DisplayName = "DELETE /api/Users/{id} - 404 ResourceNotFound quando usuário não existe")]
    public async Task DeleteUser_NotFound_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/Users/{Guid.NewGuid()}");

        await response.AssertApiErrorAsync((int)HttpStatusCode.NotFound, "ResourceNotFound");
    }
}
