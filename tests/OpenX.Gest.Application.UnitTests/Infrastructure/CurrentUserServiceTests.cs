using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using OpenX.Gest.Infrastructure.Services;

namespace OpenX.Gest.Application.UnitTests.Infrastructure;

public class CurrentUserServiceTests
{
    [Fact]
    public void Properties_WhenNoHttpContext_ShouldReturnNullsAndDefaultLanguage()
    {
        // Arrange
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(accessorMock.Object);

        // Assert
        service.UserId.Should().BeNull();
        service.Email.Should().BeNull();
        service.Role.Should().BeNull();
        service.PreferredLanguage.Should().Be("it");
    }

    [Fact]
    public void Properties_WhenUserClaimsArePresent_ShouldReturnParsedClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, "test@openx.ch"),
            new Claim(ClaimTypes.Role, "HRManager"),
            new Claim("lang", "de")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext { User = user };
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(context);

        var service = new CurrentUserService(accessorMock.Object);

        // Assert
        service.UserId.Should().Be(userId);
        service.Email.Should().Be("test@openx.ch");
        service.Role.Should().Be("HRManager");
        service.PreferredLanguage.Should().Be("de");
    }

    [Fact]
    public void UserId_WhenClaimIsNotValidGuid_ShouldReturnNull()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
        };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext { User = user };
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(a => a.HttpContext).Returns(context);

        var service = new CurrentUserService(accessorMock.Object);

        // Assert
        service.UserId.Should().BeNull();
    }
}
