using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CoolChat.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CoolChat.Server.ASPNET.Controllers;

internal class AuthenticatedResponse
{
    public string? Token { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("Login")]
    public IActionResult Login([FromBody] Login user)
    {
        if (user is null)
            return BadRequest("Invalid client request");
        
        if (user.Username == "hugo" && user.Password == "password")
        {
            SymmetricSecurityKey secretKey = new(Encoding.UTF8.GetBytes(_configuration["DefaultAuthenticationSigningKey"]!));
            SigningCredentials signingCredentials = new(secretKey, SecurityAlgorithms.HmacSha256);
            JwtSecurityToken tokenOptions = new(
                issuer: "http://localhost:3000/",
                audience: "https://localhost:3000/",
                claims: new List<Claim>(),
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: signingCredentials
            );
            string tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

            return Ok(new AuthenticatedResponse { Token = tokenString });
        }

        return Unauthorized();
    }
}
