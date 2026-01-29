using FluentAssertions;
using MYP.Domain.Entities;

namespace MYP.Domain.Tests.Entities;

public class TagTests
{
    [Fact]
    public void Tag_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var tag = new Tag();

        // Assert
        tag.Id.Should().Be(Guid.Empty);
        tag.Name.Should().BeEmpty();
        tag.Color.Should().Be("#6366f1"); // Default indigo
        tag.Snippets.Should().BeEmpty();
    }

    [Fact]
    public void Tag_WhenInitialized_ShouldHaveCorrectValues()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = "JavaScript",
            Color = "#f7df1e",
            UserId = userId
        };

        // Assert
        tag.Name.Should().Be("JavaScript");
        tag.Color.Should().Be("#f7df1e");
        tag.UserId.Should().Be(userId);
    }

    [Fact]
    public void Tag_ShouldAllowCustomColor()
    {
        // Arrange & Act
        var tag = new Tag { Color = "#ff0000" };

        // Assert
        tag.Color.Should().Be("#ff0000");
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Tag.NameMaxLength.Should().Be(50);
        Tag.ColorMaxLength.Should().Be(7);
    }

    [Fact]
    public void Tag_Snippets_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var tag = new Tag();

        // Assert
        tag.Snippets.Should().NotBeNull();
        tag.Snippets.Should().BeEmpty();
        tag.Snippets.Should().BeAssignableTo<ICollection<Snippet>>();
    }
}
