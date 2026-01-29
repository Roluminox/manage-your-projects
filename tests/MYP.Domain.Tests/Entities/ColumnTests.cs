using FluentAssertions;
using MYP.Domain.Entities;

namespace MYP.Domain.Tests.Entities;

public class ColumnTests
{
    [Fact]
    public void Column_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var column = new Column();

        // Assert
        column.Id.Should().Be(Guid.Empty);
        column.Name.Should().BeEmpty();
        column.Order.Should().Be(0);
        column.ProjectId.Should().Be(Guid.Empty);
        column.Tasks.Should().BeEmpty();
    }

    [Fact]
    public void Column_WhenInitialized_ShouldHaveCorrectValues()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var column = new Column
        {
            Id = id,
            Name = "To Do",
            Order = 0,
            ProjectId = projectId,
            CreatedAt = now
        };

        // Assert
        column.Id.Should().Be(id);
        column.Name.Should().Be("To Do");
        column.Order.Should().Be(0);
        column.ProjectId.Should().Be(projectId);
        column.CreatedAt.Should().Be(now);
        column.Tasks.Should().BeEmpty();
    }

    [Fact]
    public void Column_ShouldSupportMultipleTasks()
    {
        // Arrange
        var column = new Column { Name = "In Progress", Order = 1 };
        var task1 = new TaskItem { Title = "Task 1" };
        var task2 = new TaskItem { Title = "Task 2" };

        // Act
        column.Tasks.Add(task1);
        column.Tasks.Add(task2);

        // Assert
        column.Tasks.Should().HaveCount(2);
        column.Tasks.Should().Contain(task1);
        column.Tasks.Should().Contain(task2);
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Column.NameMaxLength.Should().Be(50);
    }
}
