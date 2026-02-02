using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MYP.API.Controllers;
using MYP.API.Tests.Common;
using MYP.Application.Features.Auth.DTOs;
using MYP.Application.Features.Projects.DTOs;
using MYP.Domain.Enums;

namespace MYP.API.Tests.Integration.Kanban;

public class ConcurrencyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ConcurrencyTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ConcurrentMoveTask_ShouldHandleRaceCondition()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Concurrent Test");
        var col1 = await CreateColumn(client, project.Id, "To Do");
        var col2 = await CreateColumn(client, project.Id, "In Progress");
        var col3 = await CreateColumn(client, project.Id, "Done");

        var task = await CreateTask(client, col1.Id, "Task to move");

        // Act - Simulate concurrent moves
        var moveRequest1 = new MoveTaskRequest(TargetColumnId: col2.Id, NewOrder: 0);
        var moveRequest2 = new MoveTaskRequest(TargetColumnId: col3.Id, NewOrder: 0);

        var task1 = client.PutAsJsonAsync($"/api/tasks/{task.Id}/move", moveRequest1);
        var task2 = client.PutAsJsonAsync($"/api/tasks/{task.Id}/move", moveRequest2);

        var responses = await Task.WhenAll(task1, task2);

        // Assert - At least one should succeed
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.NoContent);
        successCount.Should().BeGreaterThan(0, "at least one move should succeed");

        // Verify final state - task should be in exactly one column
        var projectResponse = await client.GetAsync($"/api/projects/{project.Id}");
        var projectDto = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var taskCount = projectDto!.Columns.Sum(c => c.Tasks.Count(t => t.Id == task.Id));
        taskCount.Should().Be(1, "task should exist in exactly one column");
    }

    [Fact]
    public async Task ConcurrentReorderTasks_ShouldMaintainConsistency()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Reorder Concurrent Test");
        var column = await CreateColumn(client, project.Id, "Tasks");

        var task1 = await CreateTask(client, column.Id, "Task 1");
        var task2 = await CreateTask(client, column.Id, "Task 2");
        var task3 = await CreateTask(client, column.Id, "Task 3");
        var task4 = await CreateTask(client, column.Id, "Task 4");

        // Act - Simulate concurrent reorders with different orders
        var reorderRequest1 = new ReorderTasksRequest(new List<Guid> { task4.Id, task3.Id, task2.Id, task1.Id });
        var reorderRequest2 = new ReorderTasksRequest(new List<Guid> { task1.Id, task2.Id, task3.Id, task4.Id });

        var reorderTask1 = client.PutAsJsonAsync($"/api/columns/{column.Id}/tasks/reorder", reorderRequest1);
        var reorderTask2 = client.PutAsJsonAsync($"/api/columns/{column.Id}/tasks/reorder", reorderRequest2);

        var responses = await Task.WhenAll(reorderTask1, reorderTask2);

        // Assert - Both should succeed (last one wins)
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.NoContent));

        // Verify all tasks still exist and have order values
        var projectResponse = await client.GetAsync($"/api/projects/{project.Id}");
        var projectDto = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var tasks = projectDto!.Columns.First(c => c.Id == column.Id).Tasks;
        tasks.Should().HaveCount(4, "all tasks should still exist after concurrent reorders");
    }

    [Fact]
    public async Task ConcurrentColumnReorder_ShouldMaintainConsistency()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Column Reorder Test");
        var col1 = await CreateColumn(client, project.Id, "Col 1");
        var col2 = await CreateColumn(client, project.Id, "Col 2");
        var col3 = await CreateColumn(client, project.Id, "Col 3");

        // Act - Simulate concurrent reorders
        var reorderRequest1 = new ReorderColumnsRequest(new List<Guid> { col3.Id, col2.Id, col1.Id });
        var reorderRequest2 = new ReorderColumnsRequest(new List<Guid> { col1.Id, col3.Id, col2.Id });

        var reorderTask1 = client.PutAsJsonAsync($"/api/projects/{project.Id}/columns/reorder", reorderRequest1);
        var reorderTask2 = client.PutAsJsonAsync($"/api/projects/{project.Id}/columns/reorder", reorderRequest2);

        var responses = await Task.WhenAll(reorderTask1, reorderTask2);

        // Assert - Both operations should complete
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.NoContent));

        // Verify all columns still exist
        var projectResponse = await client.GetAsync($"/api/projects/{project.Id}");
        var projectDto = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        projectDto!.Columns.Should().HaveCount(3, "all columns should still exist after concurrent reorders");
    }

    [Fact]
    public async Task ConcurrentCreateTasks_ShouldCreateAllTasks()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Create Concurrent Test");
        var column = await CreateColumn(client, project.Id, "Tasks");

        // Act - Create multiple tasks concurrently
        var createTasks = Enumerable.Range(1, 5).Select(i =>
            client.PostAsJsonAsync($"/api/columns/{column.Id}/tasks", new CreateTaskRequest(
                Title: $"Task {i}",
                Description: null,
                Priority: Priority.Medium,
                DueDate: null,
                LabelIds: null
            ))
        );

        var responses = await Task.WhenAll(createTasks);

        // Assert - All should succeed
        responses.Should().AllSatisfy(r => r.StatusCode.Should().Be(HttpStatusCode.Created));

        // Verify all tasks were created (order may have duplicates due to race condition)
        var projectResponse = await client.GetAsync($"/api/projects/{project.Id}");
        var projectDto = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var tasks = projectDto!.Columns.First(c => c.Id == column.Id).Tasks;
        tasks.Should().HaveCount(5, "all tasks should be created despite concurrent access");
        tasks.Select(t => t.Id).Distinct().Should().HaveCount(5, "all tasks should have unique IDs");
    }

    [Fact(Skip = "EF Core InMemory provider doesn't handle concurrent operations the same as a real database. This test should be run with PostgreSQL.")]
    public async Task ConcurrentMoveAndDelete_ShouldHandleGracefully()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Move Delete Test");
        var col1 = await CreateColumn(client, project.Id, "To Do");
        var col2 = await CreateColumn(client, project.Id, "Done");
        var task = await CreateTask(client, col1.Id, "Task");

        // Act - Try to move and delete the same task concurrently
        var moveRequest = new MoveTaskRequest(TargetColumnId: col2.Id, NewOrder: 0);

        var moveTask = client.PutAsJsonAsync($"/api/tasks/{task.Id}/move", moveRequest);
        var deleteTask = client.DeleteAsync($"/api/tasks/{task.Id}");

        var responses = await Task.WhenAll(moveTask, deleteTask);

        // Assert - Operations should not cause 5xx server errors
        responses.Should().AllSatisfy(r =>
            ((int)r.StatusCode).Should().BeLessThan(500, $"operation returned {r.StatusCode}"));

        // Verify task is deleted after both operations
        var taskResponse = await client.GetAsync($"/api/tasks/{task.Id}");
        taskResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

    private static async Task<ProjectDto> CreateProject(HttpClient client, string name)
    {
        var request = new CreateProjectRequest(Name: name, Description: null, Color: "#6366f1");
        var response = await client.PostAsJsonAsync("/api/projects", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProjectDto>())!;
    }

    private static async Task<ColumnDto> CreateColumn(HttpClient client, Guid projectId, string name)
    {
        var request = new CreateColumnRequest(name);
        var response = await client.PostAsJsonAsync($"/api/projects/{projectId}/columns", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ColumnDto>())!;
    }

    private static async Task<TaskItemDto> CreateTask(HttpClient client, Guid columnId, string title)
    {
        var request = new CreateTaskRequest(
            Title: title,
            Description: null,
            Priority: Priority.Medium,
            DueDate: null,
            LabelIds: null
        );
        var response = await client.PostAsJsonAsync($"/api/columns/{columnId}/tasks", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TaskItemDto>())!;
    }

    #endregion
}
