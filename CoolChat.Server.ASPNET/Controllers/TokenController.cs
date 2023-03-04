using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoolChat.Server.ASPNET.Controllers;

public class RefreshParameters
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly DataContext _dataContext;
    private readonly ITokenService _tokenService;

    public TokenController(DataContext dataContext, ITokenService tokenService)
    {
        _dataContext = dataContext;
        _tokenService = tokenService;
    }

    [HttpPost("Refresh")]
    public IActionResult Refresh([FromBody] RefreshParameters parameters)
    {
        if (parameters is null || parameters.AccessToken is null || parameters.RefreshToken is null)
            return BadRequest("Invalid client request");
        
        string accessToken = parameters.AccessToken;
        string refreshToken = parameters.RefreshToken;

        ClaimsPrincipal principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
        string? username = principal.Identity?.Name;

        if (username is null)
            return BadRequest("Invalid client request");

        Account? account = _dataContext.Accounts.SingleOrDefault(user => user.Name == username);

        if (account is null || account.RefreshToken != refreshToken || account.RefreshTokenExpiryTime <= DateTime.Now)
            return BadRequest("Invalid client request");
        
        string newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
        string newRefreshToken = _tokenService.GenerateRefreshToken();

        account.RefreshToken = newRefreshToken;
        account.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(5);
        _dataContext.SaveChanges();

        return Ok(new AuthenticatedResponse()
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
        });
    }

    [HttpPost("Revoke"), Authorize]
    public IActionResult Revoke()
    {
        string? username = User.Identity?.Name;

        if (username is null)
            return BadRequest("Invalid client request");

        Account? account = _dataContext.Accounts.FirstOrDefault(account => account.Name == username);

        if (account is null)
            return BadRequest("User not found");
        
        account.RefreshToken = null;
        _dataContext.SaveChanges();

        return NoContent();
    }
}
