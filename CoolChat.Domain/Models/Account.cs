namespace CoolChat.Domain.Models;

public class Account : BaseEntity
{
    public required string Name { get; set; }
    public virtual required string Email { get; set; }
    public required string Password { get; set; }
    public virtual required Profile Profile { get; set; }
    public virtual required ICollection<Message> Messages { get; set; }
    public virtual required ICollection<Group> Groups { get; set; }
    public virtual required Settings Settings { get; set; }
    public virtual required ICollection<Role> Roles { get; set; }
    public virtual required ICollection<Invite> SentInvites { get; set; }
    public virtual required ICollection<Invite> ReceivedInvites { get; set; }

    public virtual required ICollection<WebPushSubscription>
        WebPushSubscriptions { get; set; } // For desktop notifications

    // For session handling
    public string? RefreshToken { get; set; }
    public required DateTime RefreshTokenExpiryTime { get; set; }
}