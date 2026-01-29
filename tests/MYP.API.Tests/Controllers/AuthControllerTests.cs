using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MYP.API.Controllers;
using MYP.API.Tests.Common;
using MYP.Application.Features.Auth.DTOs;

namespace MYP.API.Tests.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_ShouldReturnOkWithUserId()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var request = new RegisterRequest(
            Email: $"test{Guid.NewGuid()}@example.com",
            Username: $"testuser{Guid.NewGuid():N}"[..20],
            Password: "Password1!",
            DisplayName: "Test User"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        var responseContent = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseContent);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var request = new RegisterRequest(
            Email: "invalid-email",
            Username: "testuser",
            Password: "Password1!",
            DisplayName: "Test User"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var request = new RegisterRequest(
            Email: "test@example.com",
            Username: "testuser",
            Password: "weak",
            DisplayName: "Test User"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var email = $"login{Guid.NewGuid()}@example.com";
        var password = "Password1!";

        await RegisterUser(client, email, password);

        var loginRequest = new LoginRequest(Email: email, Password: password);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrEmpty();
        content.RefreshToken.Should().NotBeNullOrEmpty();
        content.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var email = $"loginbad{Guid.NewGuid()}@example.com";
        await RegisterUser(client, email, "Password1!");

        var loginRequest = new LoginRequest(Email: email, Password: "WrongPassword1!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturnBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var loginRequest = new LoginRequest(
            Email: "nonexistent@example.com",
            Password: "Password1!"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var email = $"refresh{Guid.NewGuid()}@example.com";
        var authResponse = await RegisterAndLogin(client, email, "Password1!");

        var refreshRequest = new RefreshTokenRequest(RefreshToken: authResponse.RefreshToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        content.Should().NotBeNull();
        content!.AccessToken.Should().NotBeNullOrEmpty();
        content.RefreshToken.Should().NotBeNullOrEmpty();
        content.AccessToken.Should().NotBe(authResponse.AccessToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var refreshRequest = new RefreshTokenRequest(RefreshToken: "invalid_refresh_token");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_ShouldReturnBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var refreshRequest = new RefreshTokenRequest(RefreshToken: "");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GetCurrentUser Tests

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ShouldReturnUser()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var email = $"me{Guid.NewGuid()}@example.com";
        var authResponse = await RegisterAndLogin(client, email, "Password1!");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);

        // Act
        var response = await client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<UserDto>();
        content.Should().NotBeNull();
        content!.Email.Should().Be(email);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_ShouldReturnUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid_token");

        // Act
        var response = await client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Helper Methods

    private static async Task RegisterUser(HttpClient client, string email, string password)
    {
        var request = new RegisterRequest(
            Email: email,
            Username: $"user{Guid.NewGuid():N}"[..20],
            Password: password,
            DisplayName: "Test User"
        );

        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<AuthResponseDto> RegisterAndLogin(HttpClient client, string email, string password)
    {
        await RegisterUser(client, email, password);

        var loginRequest = new LoginRequest(Email: email, Password: password);
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponseDto>())!;
    }

    #endregion

    private record RegisterResponse(Guid UserId);
    private record RefreshTokenRequest(string RefreshToken);
}
