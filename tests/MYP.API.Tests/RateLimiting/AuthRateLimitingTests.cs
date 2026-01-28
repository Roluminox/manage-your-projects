using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MYP.API.Controllers;
using MYP.API.Tests.Common;

namespace MYP.API.Tests.RateLimiting;

public class AuthRateLimitingTests : IClassFixture<RateLimitingWebApplicationFactory>
{
    private readonly RateLimitingWebApplicationFactory _factory;

    public AuthRateLimitingTests(RateLimitingWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WhenRateLimitExceeded_ShouldReturn429()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var loginRequest = new LoginRequest(
            Email: "ratelimit@example.com",
            Password: "WrongPassword1!"
        );

        // Act - Make 5 requests (within limit)
        for (var i = 0; i < 5; i++)
        {
            await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        }

        // 6th request should be rate limited
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Register_WhenRateLimitExceeded_ShouldReturn429()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act - Make 5 requests (within limit)
        for (var i = 0; i < 5; i++)
        {
            var request = new RegisterRequest(
                Email: $"ratelimit{i}@example.com",
                Username: $"ratelimituser{i}",
                Password: "Password1!",
                DisplayName: "Rate Limit Test"
            );
            await client.PostAsJsonAsync("/api/auth/register", request);
        }

        // 6th request should be rate limited
        var sixthRequest = new RegisterRequest(
            Email: "ratelimit6@example.com",
            Username: "ratelimituser6",
            Password: "Password1!",
            DisplayName: "Rate Limit Test"
        );
        var response = await client.PostAsJsonAsync("/api/auth/register", sixthRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task RateLimitResponse_ShouldContainRetryAfterInfo()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var loginRequest = new LoginRequest(
            Email: "retryafter@example.com",
            Password: "WrongPassword1!"
        );

        // Act - Exhaust rate limit
        for (var i = 0; i < 5; i++)
        {
            await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        }

        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        content.Should().Contain("Too Many Requests");
        content.Should().Contain("retryAfter");
    }

    [Fact]
    public async Task GetCurrentUser_ShouldNotBeRateLimited()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act - Make more than 5 requests to /me endpoint
        var responses = new List<HttpResponseMessage>();
        for (var i = 0; i < 10; i++)
        {
            responses.Add(await client.GetAsync("/api/auth/me"));
        }

        // Assert - Should get 401 (unauthorized) not 429 (rate limited)
        // because /me is not rate limited
        responses.Should().AllSatisfy(r =>
            r.StatusCode.Should().Be(HttpStatusCode.Unauthorized));
    }
}
