using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MYP.Domain.Entities;
using MYP.Infrastructure.Identity;

namespace MYP.Infrastructure.Tests.Identity;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly JwtSettings _settings;

    public JwtServiceTests()
    {
        _settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecretKeyForTestingPurposesOnly123456!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };

        _jwtService = new JwtService(Options.Create(_settings));
    }

    #region GenerateAccessToken Tests

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainCorrectClaims()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == user.Username);
        jwtToken.Claims.Should().Contain(c => c.Type == "display_name" && c.Value == user.DisplayName);
    }

    [Fact]
    public void GenerateAccessToken_ShouldHaveCorrectIssuerAndAudience()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _jwtService.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be(_settings.Issuer);
        jwtToken.Audiences.Should().Contain(_settings.Audience);
    }

    [Fact]
    public void GenerateAccessToken_ShouldBeValidatable()
    {
        // Arrange
        var user = CreateTestUser();
        var token = _jwtService.GenerateAccessToken(user);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret))
        };

        // Act
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, validationParameters, out var validatedToken);

        // Assert
        principal.Should().NotBeNull();
        validatedToken.Should().NotBeNull();
    }

    #endregion

    #region GenerateRefreshToken Tests

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        // Act
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Assert
        var action = () => Convert.FromBase64String(refreshToken);
        action.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateUniqueTokens()
    {
        // Act
        var tokens = Enumerable.Range(0, 100)
            .Select(_ => _jwtService.GenerateRefreshToken())
            .ToList();

        // Assert
        tokens.Distinct().Should().HaveCount(100);
    }

    #endregion

    #region GetAccessTokenExpiration Tests

    [Fact]
    public void GetAccessTokenExpiration_ShouldReturnFutureDate()
    {
        // Act
        var expiration = _jwtService.GetAccessTokenExpiration();

        // Assert
        expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GetAccessTokenExpiration_ShouldBeWithinConfiguredMinutes()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var expiration = _jwtService.GetAccessTokenExpiration();

        // Assert
        var expectedMin = now.AddMinutes(_settings.AccessTokenExpirationMinutes - 1);
        var expectedMax = now.AddMinutes(_settings.AccessTokenExpirationMinutes + 1);

        expiration.Should().BeAfter(expectedMin);
        expiration.Should().BeBefore(expectedMax);
    }

    #endregion

    #region GetRefreshTokenExpiration Tests

    [Fact]
    public void GetRefreshTokenExpiration_ShouldReturnFutureDate()
    {
        // Act
        var expiration = _jwtService.GetRefreshTokenExpiration();

        // Assert
        expiration.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GetRefreshTokenExpiration_ShouldBeWithinConfiguredDays()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var expiration = _jwtService.GetRefreshTokenExpiration();

        // Assert
        var expectedMin = now.AddDays(_settings.RefreshTokenExpirationDays - 1);
        var expectedMax = now.AddDays(_settings.RefreshTokenExpirationDays + 1);

        expiration.Should().BeAfter(expectedMin);
        expiration.Should().BeBefore(expectedMax);
    }

    #endregion

    #region RefreshToken Storage Tests

    [Fact]
    public void StoreAndValidateRefreshToken_ShouldReturnUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Act
        _jwtService.StoreRefreshToken(refreshToken, userId);
        var result = _jwtService.ValidateRefreshToken(refreshToken);

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void ValidateRefreshToken_WithInvalidToken_ShouldReturnNull()
    {
        // Act
        var result = _jwtService.ValidateRefreshToken("invalid_token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void RevokeRefreshToken_ShouldInvalidateToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = _jwtService.GenerateRefreshToken();
        _jwtService.StoreRefreshToken(refreshToken, userId);

        // Act
        _jwtService.RevokeRefreshToken(refreshToken);
        var result = _jwtService.ValidateRefreshToken(refreshToken);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    private static User CreateTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            DisplayName = "Test User",
            PasswordHash = "hashed_password",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
