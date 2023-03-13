namespace CoolChat.Domain.Models;

public class Account : BaseEntity
{
    public string Name { get; set; }
    public virtual string Email { get; set; }
    public string Password { get; set; }
    public virtual Profile Profile { get; set; }
    public virtual ICollection<Message> Messages { get; set; }
    public virtual ICollection<Group> Groups { get; set; }
    public virtual ICollection<Chat> Chats { get; set; }
    public virtual Settings Settings { get; set; }
    public virtual ICollection<Role> Roles { get; set; }
    public virtual ICollection<Invite> SentInvites { get; set; }
    public virtual ICollection<Invite> ReceivedInvites { get; set; }

    // For session handling
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}