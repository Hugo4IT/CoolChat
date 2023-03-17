using System.ComponentModel.DataAnnotations;
using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoolChat.Server.ASPNET.Controllers;

public class SubscribeWebPushParameters
{
    [Required]
    public string Endpoint { get; set; }

    [Required]
    public string Key_p256dh { get; set; }

    [Required]
    public string Key_auth { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly DataContext _dataContext;

    public AccountController(IAccountService accountService, DataContext dataContext)
    {
        _accountService = accountService;
        _dataContext = dataContext;
    }

    [HttpPost("SubscribeWebPush"), Authorize]
    public async Task<IActionResult> SubscribeWebPush([FromBody] SubscribeWebPushParameters parameters)
    {
        Account account = _accountService.GetByUsername(User.Identity!.Name!)!;

        WebPushSubscription subscription = new WebPushSubscription
        {
            Endpoint = parameters.Endpoint,
            Key_p256dh = parameters.Key_p256dh,
            Key_auth = parameters.Key_auth,
        };

        // This is safe because an account can only have 10 subscriptions
        List<WebPushSubscription> subscriptions = account.WebPushSubscriptions.ToList();

        if (subscriptions.FirstOrDefault(s => s.Endpoint == subscription.Endpoint) is WebPushSubscription s) {
            if (s.Key_auth == subscription.Key_auth && s.Key_p256dh == subscription.Key_p256dh) {
                return Conflict();
            }

            subscriptions.Remove(s);
        }

        subscriptions.Add(subscription);

        account.WebPushSubscriptions = subscriptions.TakeLast(10).ToList();

        await _dataContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("UnsubscribeWebPush"), Authorize]
    public async Task<IActionResult> UnsubscribeWebPush([FromBody] SubscribeWebPushParameters parameters)
    {
        Account account = _accountService.GetByUsername(User.Identity!.Name!)!;

        WebPushSubscription subscription = new WebPushSubscription
        {
            Endpoint = parameters.Endpoint,
            Key_p256dh = parameters.Key_p256dh,
            Key_auth = parameters.Key_auth,
        };

        account.WebPushSubscriptions.Remove(subscription);

        await _dataContext.SaveChangesAsync();
        return Ok();
    }
}
