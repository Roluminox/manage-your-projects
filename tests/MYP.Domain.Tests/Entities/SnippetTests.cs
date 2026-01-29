using FluentAssertions;
using MYP.Domain.Entities;

namespace MYP.Domain.Tests.Entities;

public class SnippetTests
{
    [Fact]
    public void Snippet_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var snippet = new Snippet();

        // Assert
        snippet.Id.Should().Be(Guid.Empty);
        snippet.Title.Should().BeEmpty();
        snippet.Code.Should().BeEmpty();
        snippet.Language.Should().BeEmpty();
        snippet.Description.Should().BeNull();
        snippet.IsFavorite.Should().BeFalse();
        snippet.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Snippet_WhenInitialized_ShouldHaveCorrectValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Act
        var snippet = new Snippet
        {
            Id = Guid.NewGuid(),
            Title = "Test Snippet",
            Code = "console.log('hello');",
            Language = "javascript",
            Description = "A test snippet",
            UserId = userId,
            CreatedAt = now
        };

        // Assert
        snippet.Title.Should().Be("Test Snippet");
        snippet.Code.Should().Be("console.log('hello');");
        snippet.Language.Should().Be("javascript");
        snippet.Description.Should().Be("A test snippet");
        snippet.UserId.Should().Be(userId);
        snippet.IsFavorite.Should().BeFalse();
    }

    [Fact]
    public void ToggleFavorite_WhenNotFavorite_ShouldSetToFavorite()
    {
        // Arrange
        var snippet = new Snippet { IsFavorite = false };

        // Act
        snippet.ToggleFavorite();

        // Assert
        snippet.IsFavorite.Should().BeTrue();
        snippet.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ToggleFavorite_WhenFavorite_ShouldSetToNotFavorite()
    {
        // Arrange
        var snippet = new Snippet { IsFavorite = true };

        // Act
        snippet.ToggleFavorite();

        // Assert
        snippet.IsFavorite.Should().BeFalse();
        snippet.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_ShouldUpdateAllFieldsAndTimestamp()
    {
        // Arrange
        var snippet = new Snippet
        {
            Title = "Old Title",
            Code = "old code",
            Language = "python",
            Description = "old desc"
        };

        // Act
        snippet.Update("New Title", "new code", "typescript", "new desc");

        // Assert
        snippet.Title.Should().Be("New Title");
        snippet.Code.Should().Be("new code");
        snippet.Language.Should().Be("typescript");
        snippet.Description.Should().Be("new desc");
        snippet.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithNullDescription_ShouldClearDescription()
    {
        // Arrange
        var snippet = new Snippet { Description = "some description" };

        // Act
        snippet.Update("Title", "code", "csharp", null);

        // Assert
        snippet.Description.Should().BeNull();
    }

    [Fact]
    public void SetTags_ShouldReplaceExistingTags()
    {
        // Arrange
        var snippet = new Snippet();
        var tag1 = new Tag { Name = "tag1" };
        var tag2 = new Tag { Name = "tag2" };
        snippet.Tags.Add(tag1);

        var newTags = new List<Tag>
        {
            new Tag { Name = "newTag1" },
            new Tag { Name = "newTag2" }
        };

        // Act
        snippet.SetTags(newTags);

        // Assert
        snippet.Tags.Should().HaveCount(2);
        snippet.Tags.Should().NotContain(tag1);
        snippet.Tags.Select(t => t.Name).Should().Contain("newTag1", "newTag2");
        snippet.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetTags_WithEmptyList_ShouldClearTags()
    {
        // Arrange
        var snippet = new Snippet();
        snippet.Tags.Add(new Tag { Name = "existingTag" });

        // Act
        snippet.SetTags(new List<Tag>());

        // Assert
        snippet.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Snippet.TitleMaxLength.Should().Be(200);
        Snippet.DescriptionMaxLength.Should().Be(1000);
        Snippet.LanguageMaxLength.Should().Be(50);
    }
}
