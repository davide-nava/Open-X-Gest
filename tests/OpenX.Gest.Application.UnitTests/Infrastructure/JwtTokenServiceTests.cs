using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using OpenX.Gest.Domain.Entities;
using OpenX.Gest.Domain.Enums;
using OpenX.Gest.Infrastructure.Services;

namespace OpenX.Gest.Application.UnitTests.Infrastructure;

public class JwtTokenServiceTests
{
    [Fact]
    public void GenerateToken_WithValidEmployee_ShouldReturnValidJwtWithExpectedClaims()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Secret", "OpenX_Enterprise_Super_Secret_Key_For_Swiss_TimeTracking_2026_Minimum_32_Bytes!" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var service = new JwtTokenService(configuration);

        var employee = new Employee(
            Guid.NewGuid(),
            "Marc",
            "Favre",
            "marc.favre@openx.ch",
            "Legal",
            40.0m,
            StatutoryWeeklyLimit.Hours45,
            Oll1Regime.SimplifiedRecord,
            LanguageCode.Fr);

        // Act
        var tokenString = service.GenerateToken(employee, "HRManager");

        // Assert
        tokenString.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        token.Issuer.Should().Be("TestIssuer");
        token.Audiences.Should().Contain("TestAudience");

        token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value.Should().Be(employee.Id.ToString());
        token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.Should().Be("marc.favre@openx.ch");
        token.Claims.First(c => c.Type == ClaimTypes.Name).Value.Should().Be("Marc Favre");
        token.Claims.First(c => c.Type == ClaimTypes.Role).Value.Should().Be("HRManager");
        token.Claims.First(c => c.Type == "lang").Value.Should().Be("fr-CH");
        token.Claims.First(c => c.Type == "regime").Value.Should().Be(((int)Oll1Regime.SimplifiedRecord).ToString());
    }
}
