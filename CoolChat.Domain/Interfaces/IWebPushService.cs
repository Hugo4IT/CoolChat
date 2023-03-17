using CoolChat.Domain.Models;

namespace CoolChat.Domain.Interfaces;

public interface IWebPushService
{
    public Task SendTo(Account account, string payload);
    public Task SendTo(IEnumerable<Account> accounts, string payload);
}