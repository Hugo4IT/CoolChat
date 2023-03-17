using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using WebPush;

namespace CoolChat.Server.ASPNET.Services;

public class WebPushService : IWebPushService
{
    private readonly WebPushClient _client;

    public WebPushService()
    {
        _client = new();
    }

    public async Task SendTo(Account account, string payload)
    {
        await Task.WhenAll(account.WebPushSubscriptions!.Select(sub =>
            _client.SendNotificationAsync(new PushSubscription(sub.Endpoint, sub.Key_p256dh, sub.Key_auth), payload)));
    }

    public async Task SendTo(IEnumerable<Account> accounts, string payload)
    {
        await Task.WhenAll(accounts.Select(a => SendTo(a, payload)));
    }
}