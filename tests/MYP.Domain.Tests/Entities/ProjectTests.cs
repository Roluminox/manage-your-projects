using FluentAssertions;
using MYP.Domain.Entities;

namespace MYP.Domain.Tests.Entities;

public class ProjectTests
{
    [Fact]
    public void Project_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var project = new Project();

        // Assert
        project.Id.Should().Be(Guid.Empty);
        project.Name.Should().BeEmpty();
        project.Description.Should().BeNull();
        project.Color.Should().Be(Project.DefaultColor);
        project.Columns.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithValidData_ShouldReturnProject()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var name = "My Project";
        var description = "Project description";
        var color = "#ff5733";

        // Act
        var project = Project.Create(name, description, color, userId);

        // Assert
        project.Id.Should().NotBeEmpty();
        project.Name.Should().Be(name);
        project.Description.Should().Be(description);
        project.Color.Should().Be(color);
        project.UserId.Should().Be(userId);
        project.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        project.Columns.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithNullColor_ShouldUseDefaultColor()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var project = Project.Create("Test Project", null, null, userId);

        // Assert
        project.Color.Should().Be(Project.DefaultColor);
    }

    [Fact]
    public void Create_WithEmptyColor_ShouldUseDefaultColor()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var project = Project.Create("Test Project", null, "  ", userId);

        // Assert
        project.Color.Should().Be(Project.DefaultColor);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => Project.Create("", null, null, userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name cannot be empty*");
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var act = () => Project.Create("   ", null, null, userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name cannot be empty*");
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var longName = new string('a', Project.NameMaxLength + 1);

        // Act
        var act = () => Project.Create(longName, null, null, userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*cannot exceed {Project.NameMaxLength} characters*");
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var longDescription = new string('a', Project.DescriptionMaxLength + 1);

        // Act
        var act = () => Project.Create("Valid Name", longDescription, null, userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*cannot exceed {Project.DescriptionMaxLength} characters*");
    }

    [Fact]
    public void Create_ShouldTrimNameAndDescription()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var project = Project.Create("  My Project  ", "  Description  ", null, userId);

        // Assert
        project.Name.Should().Be("My Project");
        project.Description.Should().Be("Description");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateFields()
    {
        // Arrange
        var project = Project.Create("Original", "Original desc", "#000000", Guid.NewGuid());

        // Act
        project.Update("Updated Name", "Updated description", "#ffffff");

        // Assert
        project.Name.Should().Be("Updated Name");
        project.Description.Should().Be("Updated description");
        project.Color.Should().Be("#ffffff");
        project.UpdatedAt.Should().NotBeNull();
        project.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_WithNullDescription_ShouldClearDescription()
    {
        // Arrange
        var project = Project.Create("Name", "Some description", null, Guid.NewGuid());

        // Act
        project.Update("Name", null, null);

        // Assert
        project.Description.Should().BeNull();
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var project = Project.Create("Name", null, null, Guid.NewGuid());

        // Act
        var act = () => project.Update("", null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name cannot be empty*");
    }

    [Fact]
    public void Update_WithNameTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var project = Project.Create("Name", null, null, Guid.NewGuid());
        var longName = new string('a', Project.NameMaxLength + 1);

        // Act
        var act = () => project.Update(longName, null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*cannot exceed {Project.NameMaxLength} characters*");
    }

    [Fact]
    public void Update_WithDescriptionTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var project = Project.Create("Name", null, null, Guid.NewGuid());
        var longDescription = new string('a', Project.DescriptionMaxLength + 1);

        // Act
        var act = () => project.Update("Name", longDescription, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*cannot exceed {Project.DescriptionMaxLength} characters*");
    }

    [Fact]
    public void AddColumn_ShouldAddColumnAndUpdateTimestamp()
    {
        // Arrange
        var project = Project.Create("Name", null, null, Guid.NewGuid());
        var column = new Column { Name = "To Do", Order = 0 };

        // Act
        project.AddColumn(column);

        // Assert
        project.Columns.Should().Contain(column);
        project.Columns.Should().HaveCount(1);
        project.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Constants_ShouldHaveCorrectValues()
    {
        // Assert
        Project.NameMaxLength.Should().Be(100);
        Project.DescriptionMaxLength.Should().Be(500);
        Project.ColorMaxLength.Should().Be(7);
        Project.DefaultColor.Should().Be("#6366f1");
    }
}
