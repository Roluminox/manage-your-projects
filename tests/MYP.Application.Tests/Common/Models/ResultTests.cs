using FluentAssertions;
using MYP.Application.Common.Models;

namespace MYP.Application.Tests.Common.Models;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult_WithErrors()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var result = Result.Failure(errors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void GenericSuccess_ShouldContainValue()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void GenericFailure_ShouldThrow_WhenAccessingValue()
    {
        // Arrange
        var result = Result.Failure<string>("Error");

        // Act
        var action = () => result.Value;

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access Value on a failed result.");
    }

    [Fact]
    public void ImplicitConversion_ShouldCreateSuccessResult()
    {
        // Arrange & Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }
}
