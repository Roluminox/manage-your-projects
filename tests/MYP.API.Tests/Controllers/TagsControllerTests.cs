using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MYP.API.Tests.Common;
using MYP.Application.Features.Auth.DTOs;
using MYP.Application.Features.Snippets.DTOs;

namespace MYP.API.Tests.Controllers;

public class TagsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TagsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region CRUD Tests

    [Fact]
    public async Task CreateTag_WithValidData_ShouldReturnCreatedTag()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var request = new CreateTagRequest(Name: "JavaScript", Color: "#f7df1e");

        // Act
        var response = await client.PostAsJsonAsync("/api/tags", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<TagResponse>();
        content.Should().NotBeNull();
        content!.Name.Should().Be("JavaScript");
        content.Color.Should().Be("#f7df1e");
    }

    [Fact]
    public async Task CreateTag_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var request = new CreateTagRequest(Name: "", Color: "#f7df1e"); // Empty name

        // Act
        var response = await client.PostAsJsonAsync("/api/tags", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTag_WithDuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var request = new CreateTagRequest(Name: "DuplicateTag", Color: "#ff0000");

        // Create first tag
        await client.PostAsJsonAsync("/api/tags", request);

        // Act - Try to create duplicate
        var response = await client.PostAsJsonAsync("/api/tags", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTag_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var request = new CreateTagRequest(Name: "Test", Color: "#ff0000");

        // Act
        var response = await client.PostAsJsonAsync("/api/tags", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTags_ShouldReturnUserTags()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        await CreateTag(client, "Tag1", "#ff0000");
        await CreateTag(client, "Tag2", "#00ff00");

        // Act
        var response = await client.GetAsync("/api/tags");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<TagResponse>>();
        content.Should().NotBeNull();
        content.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task DeleteTag_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var tag = await CreateTag(client, "ToDelete", "#ff0000");

        // Act
        var response = await client.DeleteAsync($"/api/tags/{tag.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await client.GetAsync("/api/tags");
        var tags = await getResponse.Content.ReadFromJsonAsync<List<TagResponse>>();
        tags.Should().NotContain(t => t.Id == tag.Id);
    }

    [Fact]
    public async Task DeleteTag_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // Act
        var response = await client.DeleteAsync($"/api/tags/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task GetTags_ShouldNotReturnOtherUsersTags()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // User A creates a tag
        var authA = await RegisterAndLogin(client, "taga@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        await CreateTag(client, "UserATag", "#ff0000");

        // User B logs in
        var authB = await RegisterAndLogin(client, "tagb@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authB.AccessToken);

        // Act
        var response = await client.GetAsync("/api/tags");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<TagResponse>>();
        content.Should().NotBeNull();
        content.Should().NotContain(t => t.Name == "UserATag");
    }

    [Fact]
    public async Task DeleteTag_ShouldNotDeleteOtherUsersTag()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // User A creates a tag
        var authA = await RegisterAndLogin(client, "taga2@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        var tagA = await CreateTag(client, "UserAProtected", "#ff0000");

        // User B tries to delete it
        var authB = await RegisterAndLogin(client, "tagb2@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authB.AccessToken);

        // Act
        var response = await client.DeleteAsync($"/api/tags/{tagA.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify tag still exists for User A
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        var getResponse = await client.GetAsync("/api/tags");
        var tags = await getResponse.Content.ReadFromJsonAsync<List<TagResponse>>();
        tags.Should().Contain(t => t.Id == tagA.Id);
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

    private static async Task<AuthResponseDto> RegisterAndLogin(HttpClient client, string? email = null)
    {
        email ??= $"test{Guid.NewGuid()}@example.com";
        var password = "Password1!";

        await RegisterUser(client, email, password);

        var loginRequest = new LoginRequest(Email: email, Password: password);
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponseDto>())!;
    }

    private static async Task<TagResponse> CreateTag(HttpClient client, string name, string color)
    {
        var request = new CreateTagRequest(Name: name, Color: color);
        var response = await client.PostAsJsonAsync("/api/tags", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TagResponse>())!;
    }

    #endregion

    #region Helper Records

    private record RegisterRequest(string Email, string Username, string Password, string DisplayName);
    private record LoginRequest(string Email, string Password);
    private record CreateTagRequest(string Name, string Color);
    private record TagResponse(Guid Id, string Name, string Color);

    #endregion
}
