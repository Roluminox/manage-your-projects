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

public class KanbanIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public KanbanIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Project Operations (TASK-03-203)

    [Fact]
    public async Task CreateProject_WithValidData_ShouldReturnCreatedProject()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var request = new CreateProjectRequest(
            Name: "Test Project",
            Description: "A test project",
            Color: "#6366f1"
        );

        // Act
        var response = await client.PostAsJsonAsync("/api/projects", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<ProjectDto>();
        content.Should().NotBeNull();
        content!.Name.Should().Be("Test Project");
        content.Description.Should().Be("A test project");
        content.Color.Should().Be("#6366f1");
        content.Columns.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProjectById_WithColumns_ShouldReturnFullProject()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Full Project Test");
        var column = await CreateColumn(client, project.Id, "To Do");

        // Act
        var response = await client.GetAsync($"/api/projects/{project.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<ProjectDto>();
        content.Should().NotBeNull();
        content!.Columns.Should().HaveCount(1);
        content.Columns[0].Name.Should().Be("To Do");
    }

    [Fact]
    public async Task DeleteProject_ShouldReturnNoContent()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "To Delete");

        // Act
        var response = await client.DeleteAsync($"/api/projects/{project.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await client.GetAsync($"/api/projects/{project.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Column Operations (TASK-03-203)

    [Fact]
    public async Task CreateColumn_WithValidData_ShouldReturnCreatedColumn()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Column Test Project");

        var request = new CreateColumnRequest("In Progress");

        // Act
        var response = await client.PostAsJsonAsync($"/api/projects/{project.Id}/columns", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<ColumnDto>();
        content.Should().NotBeNull();
        content!.Name.Should().Be("In Progress");
        content.Order.Should().Be(0);
    }

    [Fact]
    public async Task CreateMultipleColumns_ShouldHaveCorrectOrder()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Multi Column Project");

        // Act
        await CreateColumn(client, project.Id, "To Do");
        await CreateColumn(client, project.Id, "In Progress");
        await CreateColumn(client, project.Id, "Done");

        // Assert
        var projectResponse = await client.GetAsync($"/api/projects/{project.Id}");
        var projectDto = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        projectDto!.Columns.Should().HaveCount(3);
        projectDto.Columns[0].Order.Should().Be(0);
        projectDto.Columns[1].Order.Should().Be(1);
        projectDto.Columns[2].Order.Should().Be(2);
    }

    [Fact]
    public async Task ReorderColumns_ShouldUpdateOrder()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Reorder Test");
        var col1 = await CreateColumn(client, project.Id, "Col 1");
        var col2 = await CreateColumn(client, project.Id, "Col 2");
        var col3 = await CreateColumn(client, project.Id, "Col 3");

        var reorderRequest = new ReorderColumnsRequest(new List<Guid> { col3.Id, col1.Id, col2.Id });

        // Act
        var response = await client.PutAsJsonAsync($"/api/projects/{project.Id}/columns/reorder", reorderRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var projectResponse = await client.GetAsync($"/api/projects/{project.Id}");
        var projectDto = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        projectDto!.Columns.Should().HaveCount(3);
        projectDto.Columns.OrderBy(c => c.Order).Select(c => c.Name)
            .Should().Equal("Col 3", "Col 1", "Col 2");
    }

    #endregion

    #region Task Operations (TASK-03-203)

    [Fact]
    public async Task CreateTask_WithValidData_ShouldReturnCreatedTask()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Task Test Project");
        var column = await CreateColumn(client, project.Id, "To Do");

        var request = new CreateTaskRequest(
            Title: "Test Task",
            Description: "A test task",
            Priority: Priority.High,
            DueDate: DateTime.UtcNow.AddDays(7),
            LabelIds: null
        );

        // Act
        var response = await client.PostAsJsonAsync($"/api/columns/{column.Id}/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var content = await response.Content.ReadFromJsonAsync<TaskItemDto>();
        content.Should().NotBeNull();
        content!.Title.Should().Be("Test Task");
        content.Priority.Should().Be(Priority.High);
        content.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task MoveTask_ToAnotherColumn_ShouldSucceed()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Move Task Test");
        var col1 = await CreateColumn(client, project.Id, "To Do");
        var col2 = await CreateColumn(client, project.Id, "Done");
        var task = await CreateTask(client, col1.Id, "Task to move");

        var moveRequest = new MoveTaskRequest(TargetColumnId: col2.Id, NewOrder: 0);

        // Act
        var response = await client.PutAsJsonAsync($"/api/tasks/{task.Id}/move", moveRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify task is in new column
        var projectResponse = await client.GetAsync($"/api/projects/{project.Id}");
        var projectDto = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        projectDto!.Columns.First(c => c.Id == col1.Id).Tasks.Should().BeEmpty();
        projectDto.Columns.First(c => c.Id == col2.Id).Tasks.Should().ContainSingle()
            .Which.Id.Should().Be(task.Id);
    }

    [Fact]
    public async Task ReorderTasks_WithinColumn_ShouldUpdateOrder()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Reorder Tasks Test");
        var column = await CreateColumn(client, project.Id, "To Do");
        var task1 = await CreateTask(client, column.Id, "Task 1");
        var task2 = await CreateTask(client, column.Id, "Task 2");
        var task3 = await CreateTask(client, column.Id, "Task 3");

        var reorderRequest = new ReorderTasksRequest(new List<Guid> { task3.Id, task1.Id, task2.Id });

        // Act
        var response = await client.PutAsJsonAsync($"/api/columns/{column.Id}/tasks/reorder", reorderRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var projectResponse = await client.GetAsync($"/api/projects/{project.Id}");
        var projectDto = await projectResponse.Content.ReadFromJsonAsync<ProjectDto>();

        var tasks = projectDto!.Columns.First(c => c.Id == column.Id).Tasks.OrderBy(t => t.Order).ToList();
        tasks[0].Title.Should().Be("Task 3");
        tasks[1].Title.Should().Be("Task 1");
        tasks[2].Title.Should().Be("Task 2");
    }

    [Fact]
    public async Task ArchiveTask_ShouldSetIsArchivedTrue()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Archive Test");
        var column = await CreateColumn(client, project.Id, "To Do");
        var task = await CreateTask(client, column.Id, "Task to archive");

        var archiveRequest = new ArchiveTaskRequest(Archive: true);

        // Act
        var response = await client.PutAsJsonAsync($"/api/tasks/{task.Id}/archive", archiveRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var taskResponse = await client.GetAsync($"/api/tasks/{task.Id}");
        var taskDto = await taskResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        taskDto!.IsArchived.Should().BeTrue();
    }

    #endregion

    #region Cascade Delete Tests (TASK-03-205)

    [Fact]
    public async Task DeleteProject_ShouldDeleteColumnsAndTasks()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Cascade Delete Test");
        var column = await CreateColumn(client, project.Id, "To Do");
        var task = await CreateTask(client, column.Id, "Task to cascade delete");

        // Act
        var response = await client.DeleteAsync($"/api/projects/{project.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify task is gone
        var taskResponse = await client.GetAsync($"/api/tasks/{task.Id}");
        taskResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteColumn_ShouldDeleteTasks()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var auth = await RegisterAndLogin(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var project = await CreateProject(client, "Column Cascade Test");
        var column = await CreateColumn(client, project.Id, "To Delete");
        var task = await CreateTask(client, column.Id, "Task in deleted column");

        // Act
        var response = await client.DeleteAsync($"/api/columns/{column.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify task is gone
        var taskResponse = await client.GetAsync($"/api/tasks/{task.Id}");
        taskResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify project still exists
        var projectResponse = await client.GetAsync($"/api/projects/{project.Id}");
        projectResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task GetProject_ShouldNotReturnOtherUsersProject()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // User A creates a project
        var authA = await RegisterAndLogin(client, "usera_kanban@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        var project = await CreateProject(client, "User A Project");

        // User B tries to access it
        var authB = await RegisterAndLogin(client, "userb_kanban@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authB.AccessToken);

        // Act
        var response = await client.GetAsync($"/api/projects/{project.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MoveTask_ToOtherUsersColumn_ShouldFail()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // User A creates a project with column and task
        var authA = await RegisterAndLogin(client, "usera_move@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        var projectA = await CreateProject(client, "User A Move Test");
        var columnA = await CreateColumn(client, projectA.Id, "Col A");
        var taskA = await CreateTask(client, columnA.Id, "Task A");

        // User B creates a project with column
        var authB = await RegisterAndLogin(client, "userb_move@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authB.AccessToken);
        var projectB = await CreateProject(client, "User B Move Test");
        var columnB = await CreateColumn(client, projectB.Id, "Col B");

        // User A tries to move task to User B's column
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authA.AccessToken);
        var moveRequest = new MoveTaskRequest(TargetColumnId: columnB.Id, NewOrder: 0);

        // Act
        var response = await client.PutAsJsonAsync($"/api/tasks/{taskA.Id}/move", moveRequest);

        // Assert
        // NotFound is correct - user can't see a column they don't own
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

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
