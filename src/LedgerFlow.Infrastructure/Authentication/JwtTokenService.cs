using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LedgerFlow.Infrastructure.Authentication;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public string CreateAccessToken(Guid userId, string role)
    {
        var key = configuration["Jwt:Key"] ?? "this-is-a-dev-key-change-me-in-production";
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "LedgerFlow",
            audience: configuration["Jwt:Audience"] ?? "LedgerFlowClients",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
