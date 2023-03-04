using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CoolChat.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace CoolChat.Server.ASPNET.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        SymmetricSecurityKey secretKey = new(Encoding.UTF8.GetBytes(_configuration["SecretSigningKey"]!));
        SigningCredentials signingCredentials = new(secretKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken tokenOptions = new(
            issuer: "http://localhost:3000/",
            audience: "http://localhost:3000/",
            claims: claims,
            expires: DateTime.Now.AddMinutes(5),
            signingCredentials: signingCredentials
        );

        string tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        return tokenString;
    }

    public string GenerateRefreshToken()
    {
        byte[] randomNumbers = new byte[32];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumbers);
            return Convert.ToBase64String(randomNumbers);
        }
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        TokenValidationParameters parameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretSigningKey"]!)),
            ValidateLifetime = false,
        };

        JwtSecurityTokenHandler handler = new();
        ClaimsPrincipal principal = handler.ValidateToken(token, parameters, out SecurityToken securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");
        
        return principal;
    }
}