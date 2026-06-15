using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Functional.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Auth.AuthenticateUserFeature;
using Ambev.DeveloperEvaluation.WebApi.Features.Users.CreateUser;
using FluentAssertions;

namespace Ambev.DeveloperEvaluation.Functional.Auth;

public class AuthApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "POST /api/Auth - 200 OK com credenciais válidas")]
    public async Task Authenticate_ValidCredentials_ReturnsToken()
    {
        var userRequest = ApiTestData.ValidUserRequest();
        await _client.PostAsJsonAsync("/api/Users", userRequest);

        var response = await _client.PostAsJsonAsync("/api/Auth", ApiTestData.ValidAuthRequest(userRequest));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadApiResponseAsync<AuthenticateUserResponse>();
        body!.Success.Should().BeTrue();
        body.Data!.Token.Should().NotBeNullOrWhiteSpace();
        body.Data.Email.Should().Be(userRequest.Email);
        body.Data.Name.Should().Be(userRequest.Username);
        body.Data.Role.Should().Be("Customer");
    }

    [Fact(DisplayName = "POST /api/Auth - 401 AuthenticationError com senha incorreta")]
    public async Task Authenticate_InvalidPassword_Returns401()
    {
        var userRequest = ApiTestData.ValidUserRequest();
        await _client.PostAsJsonAsync("/api/Users", userRequest);

        var response = await _client.PostAsJsonAsync("/api/Auth", new AuthenticateUserRequest
        {
            Email = userRequest.Email,
            Password = "SenhaErrada1!"
        });

        await response.AssertApiErrorAsync((int)HttpStatusCode.Unauthorized, "AuthenticationError");
    }

    [Fact(DisplayName = "POST /api/Auth - 400 ValidationError com email vazio")]
    public async Task Authenticate_InvalidRequest_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth", new AuthenticateUserRequest());

        await response.AssertApiErrorAsync((int)HttpStatusCode.BadRequest);
    }
}
