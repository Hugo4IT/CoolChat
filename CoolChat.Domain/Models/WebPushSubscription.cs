namespace CoolChat.Domain.Models;

public class WebPushSubscription : BaseEntity
{
    public required string Endpoint { get; set; }
    public required string Key_p256dh { get; set; }
    public required string Key_auth { get; set; }
}