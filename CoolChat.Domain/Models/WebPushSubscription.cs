namespace CoolChat.Domain.Models;

public class WebPushSubscription : BaseEntity
{
    public string Endpoint { get; set; }
    public string Key_p256dh { get; set; }
    public string Key_auth { get; set; }
}