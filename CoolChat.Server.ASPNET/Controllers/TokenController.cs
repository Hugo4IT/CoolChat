using CoolChat.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoolChat.Server.ASPNET.Controllers;

public class Tokens
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly DataContext _dataContext;
    private readonly ITokenService _tokenService;

    public TokenController(DataContext dataContext, IAccountService accountService, ITokenService tokenService)
    {
        _dataContext = dataContext;
        _accountService = accountService;
        _tokenService = tokenService;
    }

    [HttpPost("Refresh")]
    public async Task<IActionResult> Refresh([FromBody] Tokens parameters)
    {
        if (parameters.AccessToken == null || parameters.RefreshToken == null)
            return BadRequest("Invalid client request");

        var accessToken = parameters.AccessToken;
        var refreshToken = parameters.RefreshToken;

        var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
        var username = principal.Identity?.Name;

        if (username is null)
            return BadRequest("Invalid client request");

        var account = await _accountService.GetByUsernameAsync(username);

        if (account == null || account.RefreshToken != refreshToken || DateTime.Now <= account.RefreshTokenExpiryTime)
            return BadRequest("Invalid client request");

        var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        account.RefreshToken = newRefreshToken;
        account.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(5);
        _dataContext.SaveChanges();

        return Ok(new Tokens
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }
}