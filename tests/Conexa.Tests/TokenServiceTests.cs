using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Conexa.Domain.Constants;
using Conexa.Domain.Entities;
using Conexa.Infrastructure.Configuration;
using Conexa.Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace Conexa.Tests;

public class TokenServiceTests
{
    [Fact]
    public void CreateToken_IncludesRoleClaims()
    {
        var settings = Options.Create(new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SecretKey = "super-secret-key-with-enough-length-123456",
            ExpirationMinutes = 30
        });

        var service = new TokenService(settings);
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "admin@test.com",
            FullName = "Admin User"
        };

        var response = service.CreateToken(user, [Roles.Admin, Roles.User]);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(response.Token);
        var roles = token.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        Assert.Contains(Roles.Admin, roles);
        Assert.Contains(Roles.User, roles);
        Assert.Equal(user.Email, response.Email);
    }
}
