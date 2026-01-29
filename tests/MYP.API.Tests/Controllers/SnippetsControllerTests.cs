using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MYP.API.Tests.Common;
using MYP.Application.Features.Auth.DTOs;
using MYP.Application.Features.Snippets.DTOs;

namespace MYP.API.Tests.Controllers;

public class SnippetsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SnippetsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region CRUD Tests (TASK-02-203)

    [Fact]
    public async Task CreateSnippet_WithValidData_ShouldReturnCreatedSnippet()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var request = new CreateSnippetRequest(
            Title: "Test Snippet",
            Code: "console.log('Hello World');",
            Language: "javascript",
            Description: "A test snippet",
            TagIds: null
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/snippets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<SnippetDto>();
        content.Should().NotBeNull();
        content!.Title.Should().Be("Test Snippet");
        content.Code.Should().Be("console.log('Hello World');");
        content.Language.Should().Be("javascript");
        content.IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task CreateSnippet_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var request = new CreateSnippetRequest(
            Title: "", // Empty title should fail validation
            Code: "console.log('test');",
            Language: "javascript",
            Description: null,
            TagIds: null
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/snippets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateSnippet_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var request = new CreateSnippetRequest(
            Title: "Test",
            Code: "test",
            Language: "javascript",
            Description: null,
            TagIds: null
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/snippets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSnippetById_WithValidId_ShouldReturnSnippet()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var snippet = await CreateSnippet(client, "Get By Id Test", "const x = 1;", "typescript");

        // Act
        var response = await client.GetAsync($"/api/snippets/{snippet.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SnippetDto>();
        content.Should().NotBeNull();
        content!.Id.Should().Be(snippet.Id);
        content.Title.Should().Be("Get By Id Test");
    }

    [Fact]
    public async Task GetSnippetById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // Act
        var response = await client.GetAsync($"/api/snippets/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSnippet_WithValidData_ShouldReturnUpdatedSnippet()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var snippet = await CreateSnippet(client, "Original Title", "original code", "javascript");

        var updateRequest = new UpdateSnippetRequest(
            Title: "Updated Title",
            Code: "updated code",
            Language: "typescript",
            Description: "Updated description",
            TagIds: null
        );

        // Act
        var response = await client.PutAsJsonAsync($"/api/snippets/{snippet.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SnippetDto>();
        content.Should().NotBeNull();
        content!.Title.Should().Be("Updated Title");
        content.Code.Should().Be("updated code");
        content.Language.Should().Be("typescript");
        content.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task UpdateSnippet_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var updateRequest = new UpdateSnippetRequest(
            Title: "Test",
            Code: "test",
            Language: "javascript",
            Description: null,
            TagIds: null
        );

        // Act
        var response = await client.PutAsJsonAsync($"/api/snippets/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSnippet_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var snippet = await CreateSnippet(client, "To Delete", "delete me", "javascript");

        // Act
        var response = await client.DeleteAsync($"/api/snippets/{snippet.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await client.GetAsync($"/api/snippets/{snippet.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteSnippet_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // Act
        var response = await client.DeleteAsync($"/api/snippets/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ToggleFavorite_ShouldToggleFavoriteStatus()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var snippet = await CreateSnippet(client, "Favorite Test", "test", "javascript");

        // Act - First toggle (should set to true)
        var response1 = await client.PatchAsync($"/api/snippets/{snippet.Id}/favorite", null);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        var result1 = await response1.Content.ReadFromJsonAsync<FavoriteResponse>();
        result1!.IsFavorite.Should().BeTrue();

        // Act - Second toggle (should set back to false)
        var response2 = await client.PatchAsync($"/api/snippets/{snippet.Id}/favorite", null);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        var result2 = await response2.Content.ReadFromJsonAsync<FavoriteResponse>();
        result2!.IsFavorite.Should().BeFalse();
    }

    #endregion

    #region Search Tests (TASK-02-204)

    [Fact]
    public async Task SearchSnippets_WithMatchingTerm_ShouldReturnResults()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        await CreateSnippet(client, "Search Test Alpha", "alpha code", "javascript");
        await CreateSnippet(client, "Search Test Beta", "beta code", "typescript");
        await CreateSnippet(client, "Different Title", "gamma code", "python");

        // Act
        var response = await client.GetAsync("/api/snippets/search?q=Search Test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SnippetListResponse>();
        content.Should().NotBeNull();
        content!.Items.Should().HaveCountGreaterThanOrEqualTo(2);
        content.Items.Should().AllSatisfy(s => s.Title.Should().Contain("Search Test"));
    }

    [Fact]
    public async Task SearchSnippets_WithNoMatchingTerm_ShouldReturnEmptyResults()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        await CreateSnippet(client, "Normal Title", "code", "javascript");

        // Act
        var response = await client.GetAsync("/api/snippets/search?q=NonExistentXYZ123");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SnippetListResponse>();
        content.Should().NotBeNull();
        content!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchSnippets_InCodeContent_ShouldReturnResults()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        await CreateSnippet(client, "Regular Title", "function uniqueSearchTermXYZ() {}", "javascript");

        // Act
        var response = await client.GetAsync("/api/snippets/search?q=uniqueSearchTermXYZ");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SnippetListResponse>();
        content.Should().NotBeNull();
        content!.Items.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    #endregion

    #region Pagination Tests (TASK-02-205)

    [Fact]
    public async Task GetSnippets_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // Create 15 snippets
        for (int i = 0; i < 15; i++)
        {
            await CreateSnippet(client, $"Pagination Test {i}", $"code {i}", "javascript");
        }

        // Act - Get first page with 5 items
        var response1 = await client.GetAsync("/api/snippets?page=1&pageSize=5");
        var page1 = await response1.Content.ReadFromJsonAsync<SnippetListResponse>();

        // Act - Get second page
        var response2 = await client.GetAsync("/api/snippets?page=2&pageSize=5");
        var page2 = await response2.Content.ReadFromJsonAsync<SnippetListResponse>();

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        page1!.Items.Should().HaveCount(5);
        page1.Page.Should().Be(1);
        page1.PageSize.Should().Be(5);
        page1.TotalPages.Should().BeGreaterThanOrEqualTo(3);

        page2!.Items.Should().HaveCount(5);
        page2.Page.Should().Be(2);

        // Ensure pages have different items
        page1.Items.Select(s => s.Id).Should().NotIntersectWith(page2.Items.Select(s => s.Id));
    }

    [Fact]
    public async Task GetSnippets_WithLanguageFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        await CreateSnippet(client, "JS Snippet", "js code", "javascript");
        await CreateSnippet(client, "TS Snippet", "ts code", "typescript");
        await CreateSnippet(client, "Python Snippet", "py code", "python");

        // Act
        var response = await client.GetAsync("/api/snippets?language=typescript");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SnippetListResponse>();
        content.Should().NotBeNull();
        content!.Items.Should().AllSatisfy(s => s.Language.Should().Be("typescript"));
    }

    [Fact]
    public async Task GetSnippets_WithFavoriteFilter_ShouldReturnOnlyFavorites()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var snippet1 = await CreateSnippet(client, "Favorite Snippet", "code", "javascript");
        await CreateSnippet(client, "Non-Favorite Snippet", "code", "javascript");

        // Toggle favorite
        await client.PatchAsync($"/api/snippets/{snippet1.Id}/favorite", null);

        // Act
        var response = await client.GetAsync("/api/snippets?isFavorite=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SnippetListResponse>();
        content.Should().NotBeNull();
        content!.Items.Should().AllSatisfy(s => s.IsFavorite.Should().BeTrue());
    }

    #endregion

    #region Tags Management Tests (TASK-02-206)

    [Fact]
    public async Task CreateSnippet_WithTags_ShouldAssociateTags()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // Create tags first
        var tag1 = await CreateTag(client, "JavaScript", "#f7df1e");
        var tag2 = await CreateTag(client, "Frontend", "#3178c6");

        var request = new CreateSnippetRequest(
            Title: "Tagged Snippet",
            Code: "const x = 1;",
            Language: "javascript",
            Description: null,
            TagIds: new List<Guid> { tag1.Id, tag2.Id }
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/snippets", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<SnippetDto>();
        content.Should().NotBeNull();
        content!.Tags.Should().HaveCount(2);
        content.Tags.Should().Contain(t => t.Name == "JavaScript");
        content.Tags.Should().Contain(t => t.Name == "Frontend");
    }

    [Fact]
    public async Task UpdateSnippet_ShouldUpdateTags()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var tag1 = await CreateTag(client, "OldTag", "#ff0000");
        var tag2 = await CreateTag(client, "NewTag", "#00ff00");

        var snippet = await CreateSnippetWithTags(client, "Tag Update Test", "code", "javascript", new[] { tag1.Id });

        var updateRequest = new UpdateSnippetRequest(
            Title: "Tag Update Test",
            Code: "code",
            Language: "javascript",
            Description: null,
            TagIds: new List<Guid> { tag2.Id }
        );

        // Act
        var response = await client.PutAsJsonAsync($"/api/snippets/{snippet.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SnippetDto>();
        content.Should().NotBeNull();
        content!.Tags.Should().HaveCount(1);
        content.Tags.Should().Contain(t => t.Name == "NewTag");
        content.Tags.Should().NotContain(t => t.Name == "OldTag");
    }

    [Fact]
    public async Task GetSnippets_WithTagFilter_ShouldReturnSnippetsWithTag()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var tag = await CreateTag(client, "FilterTag", "#ff0000");
        await CreateSnippetWithTags(client, "With Tag", "code1", "javascript", new[] { tag.Id });
        await CreateSnippet(client, "Without Tag", "code2", "javascript");

        // Act
        var response = await client.GetAsync($"/api/snippets?tagId={tag.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SnippetListResponse>();
        content.Should().NotBeNull();
        content!.Items.Should().AllSatisfy(s => s.Tags.Should().Contain(t => t.Id == tag.Id));
    }

    #endregion

    #region Authorization Tests (TASK-02-207)

    [Fact]
    public async Task GetSnippets_ShouldNotReturnOtherUsersSnippets()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // User A creates a snippet
        var authA = await RegisterAndLogin(client, "usera@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        var snippetA = await CreateSnippet(client, "User A Snippet", "code A", "javascript");

        // User B logs in
        var authB = await RegisterAndLogin(client, "userb@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authB.AccessToken);

        // Act - User B gets their snippets
        var response = await client.GetAsync("/api/snippets");

        // Assert - Should not see User A's snippet
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<SnippetListResponse>();
        content.Should().NotBeNull();
        content!.Items.Should().NotContain(s => s.Id == snippetA.Id);
    }

    [Fact]
    public async Task GetSnippetById_ShouldNotReturnOtherUsersSnippet()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // User A creates a snippet
        var authA = await RegisterAndLogin(client, "usera2@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        var snippetA = await CreateSnippet(client, "User A Private", "secret code", "javascript");

        // User B tries to access it
        var authB = await RegisterAndLogin(client, "userb2@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authB.AccessToken);

        // Act
        var response = await client.GetAsync($"/api/snippets/{snippetA.Id}");

        // Assert - Should not be found (user isolation)
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateSnippet_ShouldNotUpdateOtherUsersSnippet()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // User A creates a snippet
        var authA = await RegisterAndLogin(client, "usera3@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        var snippetA = await CreateSnippet(client, "Original", "original code", "javascript");

        // User B tries to update it
        var authB = await RegisterAndLogin(client, "userb3@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authB.AccessToken);

        var updateRequest = new UpdateSnippetRequest(
            Title: "Hacked",
            Code: "hacked code",
            Language: "javascript",
            Description: null,
            TagIds: null
        );

        // Act
        var response = await client.PutAsJsonAsync($"/api/snippets/{snippetA.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify original is unchanged
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        var getResponse = await client.GetAsync($"/api/snippets/{snippetA.Id}");
        var content = await getResponse.Content.ReadFromJsonAsync<SnippetDto>();
        content!.Title.Should().Be("Original");
    }

    [Fact]
    public async Task DeleteSnippet_ShouldNotDeleteOtherUsersSnippet()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // User A creates a snippet
        var authA = await RegisterAndLogin(client, "usera4@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        var snippetA = await CreateSnippet(client, "To Protect", "protected code", "javascript");

        // User B tries to delete it
        var authB = await RegisterAndLogin(client, "userb4@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authB.AccessToken);

        // Act
        var response = await client.DeleteAsync($"/api/snippets/{snippetA.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify snippet still exists for User A
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        var getResponse = await client.GetAsync($"/api/snippets/{snippetA.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
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

    private static async Task<SnippetDto> CreateSnippet(HttpClient client, string title, string code, string language)
    {
        var request = new CreateSnippetRequest(
            Title: title,
            Code: code,
            Language: language,
            Description: null,
            TagIds: null
        );

        var response = await client.PostAsJsonAsync("/api/snippets", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SnippetDto>())!;
    }

    private static async Task<SnippetDto> CreateSnippetWithTags(HttpClient client, string title, string code, string language, Guid[] tagIds)
    {
        var request = new CreateSnippetRequest(
            Title: title,
            Code: code,
            Language: language,
            Description: null,
            TagIds: tagIds.ToList()
        );

        var response = await client.PostAsJsonAsync("/api/snippets", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<SnippetDto>())!;
    }

    private static async Task<TagDto> CreateTag(HttpClient client, string name, string color)
    {
        var request = new CreateTagRequest(Name: name, Color: color);
        var response = await client.PostAsJsonAsync("/api/tags", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TagDto>())!;
    }

    #endregion

    #region Helper Records

    private record RegisterRequest(string Email, string Username, string Password, string DisplayName);
    private record LoginRequest(string Email, string Password);
    private record FavoriteResponse(bool IsFavorite);
    private record CreateTagRequest(string Name, string Color);

    #endregion
}
