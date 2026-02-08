using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MYP.API.Tests.Common;
using MYP.Application.Features.Auth.DTOs;
using MYP.Application.Features.AuditLogs.DTOs;
using MYP.Application.Features.Snippets.DTOs;
using MYP.Domain.Enums;

namespace MYP.API.Tests.Integration.AuditLogs;

public class AuditLogsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuditLogsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAuditLogs_WhenAuthenticated_ShouldReturnOk()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // Act
        var response = await client.GetAsync("/api/audit-logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuditLogListResponse>();
        content.Should().NotBeNull();
        content!.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetAuditLogs_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/audit-logs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuditLogs_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // Create some snippets to generate audit logs
        for (int i = 0; i < 5; i++)
        {
            await CreateSnippet(client, $"Snippet {i}");
        }

        // Act
        var response = await client.GetAsync("/api/audit-logs?page=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuditLogListResponse>();
        content.Should().NotBeNull();
        content!.Items.Should().HaveCount(2);
        content.TotalCount.Should().BeGreaterThanOrEqualTo(5);
        content.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetAuditLogs_WithEntityTypeFilter_ShouldFilterResults()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        await CreateSnippet(client, "Filter Test Snippet");

        // Act
        var response = await client.GetAsync("/api/audit-logs?entityType=Snippet");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuditLogListResponse>();
        content.Should().NotBeNull();
        content!.Items.Should().AllSatisfy(item => item.EntityType.Should().Be("Snippet"));
    }

    [Fact]
    public async Task GetAuditLogs_WithActionFilter_ShouldFilterResults()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        await CreateSnippet(client, "Action Filter Test");

        // Act
        var response = await client.GetAsync($"/api/audit-logs?action={AuditAction.Created}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuditLogListResponse>();
        content.Should().NotBeNull();
        content!.Items.Should().AllSatisfy(item => item.Action.Should().Be(AuditAction.Created));
    }

    [Fact]
    public async Task GetAuditLogsByEntity_ShouldReturnEntityHistory()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var snippet = await CreateSnippet(client, "Entity History Test");

        // Act
        var response = await client.GetAsync($"/api/audit-logs/Snippet/{snippet!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<AuditLogDto>>();
        content.Should().NotBeNull();
        content.Should().NotBeEmpty();
        content!.Should().AllSatisfy(item =>
        {
            item.EntityType.Should().Be("Snippet");
            item.EntityId.Should().Be(snippet.Id);
        });
    }

    [Fact]
    public async Task GetAuditLogs_ShouldOnlyReturnCurrentUserLogs()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // User A creates a snippet
        var authA = await RegisterAndLogin(client, "audita@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        await CreateSnippet(client, "User A Snippet");

        // User B queries logs
        var authB = await RegisterAndLogin(client, "auditb@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authB.AccessToken);

        // Act
        var response = await client.GetAsync("/api/audit-logs?entityType=Snippet");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AuditLogListResponse>();
        content.Should().NotBeNull();
        // User B should not see User A's audit logs
        content!.Items.Should().NotContain(item => item.UserDisplayName == "Test User A");
    }

    #region Helper Methods

    private static async Task<AuthResponseDto> RegisterAndLogin(HttpClient client, string? email = null)
    {
        email ??= $"test{Guid.NewGuid()}@example.com";
        var password = "Password1!";

        var registerRequest = new { Email = email, Username = $"user{Guid.NewGuid():N}"[..20], Password = password, DisplayName = "Test User" };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginRequest = new { Email = email, Password = password };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        return (await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>())!;
    }

    private static async Task<SnippetDto?> CreateSnippet(HttpClient client, string title)
    {
        var request = new CreateSnippetRequest(
            Title: title,
            Code: "console.log('test');",
            Language: "javascript",
            Description: null,
            TagIds: null
        );
        var response = await client.PostAsJsonAsync("/api/snippets", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SnippetDto>();
    }

    #endregion
}
