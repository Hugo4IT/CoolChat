using System.Security.Claims;
using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static CoolChat.Server.ASPNET.Validation;

namespace CoolChat.Server.ASPNET.Controllers;

public class LoginParameters
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IConfiguration _configuration;
    private readonly DataContext _dataContext;
    private readonly ILogger<AuthController> _logger;
    private readonly ITokenService _tokenService;

    public AuthController(ILogger<AuthController> logger, IConfiguration configuration, DataContext dataContext,
        ITokenService tokenService, IAccountService accountService)
    {
        _logger = logger;
        _configuration = configuration;
        _dataContext = dataContext;
        _tokenService = tokenService;
        _accountService = accountService;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginParameters parameters)
    {
        if (parameters == null || parameters.Username == null || parameters.Password == null)
            return BadRequest("Invalid client request");

        var result = await _accountService.LoginAsync(parameters.Username, parameters.Password);

        if (!result.Success)
            return Ok(result);

        return Ok(await StartSession(result.Valid()!.Value));
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] LoginParameters parameters)
    {
        if (parameters == null || parameters.Username == null || parameters.Password == null)
            return BadRequest("Invalid client request");

        var result = await _accountService.CreateAccountAsync(parameters.Username, parameters.Password);

        if (!result.Success)
            return Ok(result);

        return Ok(await StartSession(result.Valid()!.Value));
    }

    [HttpPost("Logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var account = (await _accountService.GetByUsernameAsync(User.Identity!.Name!))!;

        account.RefreshToken = null;

        await _dataContext.SaveChangesAsync();

        return Ok();
    }

    private async Task<IValidationResult<Tokens>> StartSession(Account account)
    {
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name, account.Name),
        };

        var accessToken = _tokenService.GenerateAccessToken(claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        account.RefreshToken = refreshToken;
        account.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(5);

        await _dataContext.SaveChangesAsync();

        return Valid(new Tokens
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }
}