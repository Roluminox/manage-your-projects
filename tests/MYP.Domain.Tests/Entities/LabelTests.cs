using FluentAssertions;
using MYP.Domain.Entities;

namespace MYP.Domain.Tests.Entities;

public class LabelTests
{
    [Fact]
    public void Label_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var label = new Label();

        // Assert
        label.Id.Should().Be(Guid.Empty);
        label.Name.Should().BeEmpty();
        label.Color.Should().Be("#6366f1");
        label.ProjectId.Should().Be(Guid.Empty);
        label.Tasks.Should().BeEmpty();
    }

    [Fact]
    public void Label_WhenInitialized_ShouldHaveCorrectValues()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var label = new Label
        {
            Id = id,
            Name = "Bug",
            Color = "#ff0000",
            ProjectId = projectId,
            CreatedAt = now
        };

        // Assert
        label.Id.Should().Be(id);
        label.Name.Should().Be("Bug");
        label.Color.Should().Be("#ff0000");
        label.ProjectId.Should().Be(projectId);
        label.CreatedAt.Should().Be(now);
        label.Tasks.Should().BeEmpty();
    }

    [Fact]
    public void Label_ShouldSupportMultipleTasks()
    {
        // Arrange
        var label = new Label { Name = "Urgent", Color = "#ff0000" };
        var task1 = new TaskItem { Title = "Task 1" };
        var task2 = new TaskItem { Title = "Task 2" };

        // Act
        label.Tasks.Add(task1);
        label.Tasks.Add(task2);

        // Assert
        label.Tasks.Should().HaveCount(2);
        label.Tasks.Should().Contain(task1);
        label.Tasks.Should().Contain(task2);
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Label.NameMaxLength.Should().Be(30);
        Label.ColorMaxLength.Should().Be(7);
    }
}
