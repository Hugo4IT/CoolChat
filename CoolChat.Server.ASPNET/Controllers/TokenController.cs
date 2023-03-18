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
    private readonly IAccountService _accountService;
    private readonly ITokenService _tokenService;

    public TokenController(DataContext dataContext, IAccountService accountService, ITokenService tokenService)
    {
        _dataContext = dataContext;
        _accountService = accountService;
        _tokenService = tokenService;
    }

    [HttpPost("Refresh")]
    public IActionResult Refresh([FromBody] RefreshParameters parameters)
    {
        if (parameters == null || parameters.AccessToken == null || parameters.RefreshToken == null)
            return BadRequest("Invalid client request");
        
        string accessToken = parameters.AccessToken;
        string refreshToken = parameters.RefreshToken;

        ClaimsPrincipal principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
        string? username = principal.Identity?.Name;

        if (username is null)
            return BadRequest("Invalid client request");

        Account? account = _accountService.GetByUsername(username);

        if (account == null || account.RefreshToken != refreshToken || DateTime.Now <= account.RefreshTokenExpiryTime)
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
}
