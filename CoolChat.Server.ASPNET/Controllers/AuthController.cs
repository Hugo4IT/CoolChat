using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CoolChat.Server.ASPNET.Controllers;

public class LoginParameters
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

internal class AuthenticatedResponse
{
    public bool Success { get; set; }
    public string? UsernameError { get; set; }
    public string? PasswordError { get; set; }

    public string Token { get; set; } = "";
    public string RefreshToken { get; set; } = "";
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly DataContext _dataContext;
    private readonly ITokenService _tokenService;
    private readonly IAccountService _accountService;

    public AuthController(ILogger<AuthController> logger, IConfiguration configuration, DataContext dataContext, ITokenService tokenService, IAccountService accountService)
    {
        _logger = logger;
        _configuration = configuration;
        _dataContext = dataContext;
        _tokenService = tokenService;
        _accountService = accountService;
    }

    [HttpPost("Login")]
    public IActionResult Login([FromBody] LoginParameters parameters)
    {
        if (parameters == null || parameters.Username == null || parameters.Password == null)
            return BadRequest("Invalid client request");

        LoginResult result = _accountService.Login(parameters.Username, parameters.Password);

        if (!result.Success)
        {
            return Ok(new AuthenticatedResponse
            {
                Success = false,
                UsernameError = result.UsernameError,
                PasswordError = result.PasswordError,
            });
        }

        return Ok(StartSession(result.Account!));
    }

    [HttpPost("Register")]
    public IActionResult Register([FromBody] LoginParameters parameters)
    {
        if (parameters == null || parameters.Username == null || parameters.Password == null)
            return BadRequest("Invalid client request");
        
        try
        {
            LoginResult result = _accountService.CreateAccount(parameters.Username, parameters.Password);

            if (!result.Success)
            {
                return Ok(new AuthenticatedResponse
                {
                    Success = false,
                    UsernameError = result.UsernameError,
                    PasswordError = result.PasswordError,
                });
            }

            return Ok(StartSession(result.Account!));
        }
        catch (ArgumentException e)
        {
            Debug.WriteLine(e.Message, "error");
            return BadRequest("A user with this name already exists");
        }
    }

    [HttpPost("Logout"), Authorize]
    public async Task<IActionResult> Logout()
    {
        Account account = _accountService.GetByUsername(User.Identity!.Name!)!;

        account.RefreshToken = null;

        await _dataContext.SaveChangesAsync();

        return Ok();
    }

    private AuthenticatedResponse StartSession(Account account)
    {
        List<Claim> claims = new()
        {
            new(ClaimTypes.Name, account.Name),
        };

        string accessToken = _tokenService.GenerateAccessToken(claims);
        string refreshToken = _tokenService.GenerateRefreshToken();

        account.RefreshToken = refreshToken;
        account.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(5);

        _dataContext.SaveChanges();

        return new AuthenticatedResponse {
            Success = true,
            Token = accessToken,
            RefreshToken = refreshToken,
        };
    }
}
