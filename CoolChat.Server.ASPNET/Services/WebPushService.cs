using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using WebPush;

namespace CoolChat.Server.ASPNET.Services;

public class WebPushService : IWebPushService
{
    private readonly WebPushClient _client;
    private readonly ILogger<WebPushService> _logger;

    public WebPushService(ILogger<WebPushService> logger)
    {
        _client = new WebPushClient();
        _logger = logger;
    }

    public async Task SendTo(Account account, string payload)
    {
        List<WebPushSubscription> toRemove = new();

        await Task.WhenAll(account.WebPushSubscriptions!.Select(async sub =>
        {
            try
            {
                await _client.SendNotificationAsync(new PushSubscription(sub.Endpoint, sub.Key_p256dh, sub.Key_auth),
                    payload);
            }
            catch (Exception e)
            {
                _logger.LogInformation("WebPushSubscription gave error, removing from list: {message}", e.Message);
                toRemove.Add(sub);
            }
        }));

        foreach (var sub in toRemove)
            account.WebPushSubscriptions.Remove(sub);
    }

    public async Task SendTo(IEnumerable<Account> accounts, string payload)
    {
        await Task.WhenAll(accounts.Select(a => SendTo(a, payload)));
    }
}