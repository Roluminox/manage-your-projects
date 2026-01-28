using FluentAssertions;
using MYP.API.Tests.Common;
using System.Net;

namespace MYP.API.Tests;

public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public HealthCheckTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ApiRoot_ShouldReturn_NotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnknownApiEndpoint_ShouldReturn_NotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/unknown-endpoint");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
