using FluentAssertions;
using MYP.Domain.Entities;

namespace MYP.Domain.Tests.Entities;

public class ChecklistItemTests
{
    [Fact]
    public void ChecklistItem_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var item = new ChecklistItem();

        // Assert
        item.Id.Should().Be(Guid.Empty);
        item.Text.Should().BeEmpty();
        item.IsCompleted.Should().BeFalse();
        item.Order.Should().Be(0);
        item.TaskItemId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ChecklistItem_WhenInitialized_ShouldHaveCorrectValues()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var item = new ChecklistItem
        {
            Id = id,
            Text = "Review code",
            IsCompleted = false,
            Order = 0,
            TaskItemId = taskId,
            CreatedAt = now
        };

        // Assert
        item.Id.Should().Be(id);
        item.Text.Should().Be("Review code");
        item.IsCompleted.Should().BeFalse();
        item.Order.Should().Be(0);
        item.TaskItemId.Should().Be(taskId);
        item.CreatedAt.Should().Be(now);
    }

    [Fact]
    public void Toggle_WhenNotCompleted_ShouldSetToCompleted()
    {
        // Arrange
        var item = new ChecklistItem { IsCompleted = false };

        // Act
        item.Toggle();

        // Assert
        item.IsCompleted.Should().BeTrue();
        item.UpdatedAt.Should().NotBeNull();
        item.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Toggle_WhenCompleted_ShouldSetToNotCompleted()
    {
        // Arrange
        var item = new ChecklistItem { IsCompleted = true };

        // Act
        item.Toggle();

        // Assert
        item.IsCompleted.Should().BeFalse();
        item.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Toggle_MultipleTimes_ShouldAlternateState()
    {
        // Arrange
        var item = new ChecklistItem { IsCompleted = false };

        // Act & Assert
        item.Toggle();
        item.IsCompleted.Should().BeTrue();

        item.Toggle();
        item.IsCompleted.Should().BeFalse();

        item.Toggle();
        item.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        ChecklistItem.TextMaxLength.Should().Be(200);
    }
}
