using FluentAssertions;
using MYP.Domain.Entities;
using MYP.Domain.Enums;

namespace MYP.Domain.Tests.Entities;

public class TaskItemTests
{
    [Fact]
    public void TaskItem_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var task = new TaskItem();

        // Assert
        task.Id.Should().Be(Guid.Empty);
        task.Title.Should().BeEmpty();
        task.Description.Should().BeNull();
        task.Priority.Should().Be(Priority.Medium);
        task.DueDate.Should().BeNull();
        task.Order.Should().Be(0);
        task.IsArchived.Should().BeFalse();
        task.ColumnId.Should().Be(Guid.Empty);
        task.Labels.Should().BeEmpty();
        task.Checklists.Should().BeEmpty();
    }

    [Fact]
    public void TaskItem_WhenInitialized_ShouldHaveCorrectValues()
    {
        // Arrange
        var columnId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var dueDate = DateTime.UtcNow.AddDays(7);

        // Act
        var task = new TaskItem
        {
            Id = id,
            Title = "Complete feature",
            Description = "Implement the new feature",
            Priority = Priority.High,
            DueDate = dueDate,
            Order = 2,
            IsArchived = false,
            ColumnId = columnId,
            CreatedAt = now
        };

        // Assert
        task.Id.Should().Be(id);
        task.Title.Should().Be("Complete feature");
        task.Description.Should().Be("Implement the new feature");
        task.Priority.Should().Be(Priority.High);
        task.DueDate.Should().Be(dueDate);
        task.Order.Should().Be(2);
        task.IsArchived.Should().BeFalse();
        task.ColumnId.Should().Be(columnId);
        task.CreatedAt.Should().Be(now);
    }

    [Fact]
    public void TaskItem_ShouldSupportAllPriorityLevels()
    {
        // Arrange & Act & Assert
        var lowTask = new TaskItem { Priority = Priority.Low };
        var mediumTask = new TaskItem { Priority = Priority.Medium };
        var highTask = new TaskItem { Priority = Priority.High };
        var criticalTask = new TaskItem { Priority = Priority.Critical };

        lowTask.Priority.Should().Be(Priority.Low);
        mediumTask.Priority.Should().Be(Priority.Medium);
        highTask.Priority.Should().Be(Priority.High);
        criticalTask.Priority.Should().Be(Priority.Critical);
    }

    [Fact]
    public void TaskItem_ShouldSupportLabels()
    {
        // Arrange
        var task = new TaskItem { Title = "Task with labels" };
        var label1 = new Label { Name = "Bug", Color = "#ff0000" };
        var label2 = new Label { Name = "Feature", Color = "#00ff00" };

        // Act
        task.Labels.Add(label1);
        task.Labels.Add(label2);

        // Assert
        task.Labels.Should().HaveCount(2);
        task.Labels.Should().Contain(label1);
        task.Labels.Should().Contain(label2);
    }

    [Fact]
    public void TaskItem_ShouldSupportChecklists()
    {
        // Arrange
        var task = new TaskItem { Title = "Task with checklist" };
        var item1 = new ChecklistItem { Text = "Step 1", Order = 0 };
        var item2 = new ChecklistItem { Text = "Step 2", Order = 1 };

        // Act
        task.Checklists.Add(item1);
        task.Checklists.Add(item2);

        // Assert
        task.Checklists.Should().HaveCount(2);
        task.Checklists.Should().Contain(item1);
        task.Checklists.Should().Contain(item2);
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        TaskItem.TitleMaxLength.Should().Be(200);
        TaskItem.DescriptionMaxLength.Should().Be(2000);
    }
}
