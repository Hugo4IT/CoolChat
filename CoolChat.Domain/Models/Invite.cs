namespace CoolChat.Domain.Models;

public enum InviteType
{
    DirectMessage,
    Group
}

public class Invite : BaseEntity
{
    public virtual required Account From { get; set; }
    public virtual required Account To { get; set; }
    public int InvitedId { get; set; }
    public InviteType Type { get; set; }
    public DateTime Expires { get; set; }
}