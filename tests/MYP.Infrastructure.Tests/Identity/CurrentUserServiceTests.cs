using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MYP.Infrastructure.Identity;
using NSubstitute;

namespace MYP.Infrastructure.Tests.Identity;

public class CurrentUserServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CurrentUserService _currentUserService;

    public CurrentUserServiceTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _currentUserService = new CurrentUserService(_httpContextAccessor);
    }

    #region IsAuthenticated Tests

    [Fact]
    public void IsAuthenticated_WhenUserIsAuthenticated_ShouldReturnTrue()
    {
        // Arrange
        var httpContext = CreateHttpContext(isAuthenticated: true);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _currentUserService.IsAuthenticated;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_WhenUserIsNotAuthenticated_ShouldReturnFalse()
    {
        // Arrange
        var httpContext = CreateHttpContext(isAuthenticated: false);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _currentUserService.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAuthenticated_WhenHttpContextIsNull_ShouldReturnFalse()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = _currentUserService.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserId Tests

    [Fact]
    public void UserId_WhenClaimExists_ShouldReturnUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var httpContext = CreateHttpContext(
            isAuthenticated: true,
            userId: expectedUserId
        );
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _currentUserService.UserId;

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void UserId_WhenSubClaimExists_ShouldReturnUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var httpContext = CreateHttpContextWithSubClaim(
            isAuthenticated: true,
            userId: expectedUserId
        );
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _currentUserService.UserId;

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void UserId_WhenNoUserIdClaim_ShouldReturnNull()
    {
        // Arrange
        var httpContext = CreateHttpContext(isAuthenticated: true);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _currentUserService.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UserId_WhenHttpContextIsNull_ShouldReturnNull()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = _currentUserService.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void UserId_WhenClaimIsInvalidGuid_ShouldReturnNull()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "not-a-guid")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = _currentUserService.UserId;

        // Assert
        result.Should().BeNull();
    }

    #endregion

    private static HttpContext CreateHttpContext(bool isAuthenticated, Guid? userId = null)
    {
        var claims = new List<Claim>();

        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }

        var identity = isAuthenticated
            ? new ClaimsIdentity(claims, "TestAuth")
            : new ClaimsIdentity(claims);

        var principal = new ClaimsPrincipal(identity);
        return new DefaultHttpContext { User = principal };
    }

    private static HttpContext CreateHttpContextWithSubClaim(bool isAuthenticated, Guid? userId = null)
    {
        var claims = new List<Claim>();

        if (userId.HasValue)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()));
        }

        var identity = isAuthenticated
            ? new ClaimsIdentity(claims, "TestAuth")
            : new ClaimsIdentity(claims);

        var principal = new ClaimsPrincipal(identity);
        return new DefaultHttpContext { User = principal };
    }
}
