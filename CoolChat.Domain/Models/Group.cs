namespace CoolChat.Domain.Models;

public class Group : BaseEntity
{
    public required string Name { get; set; }
    public virtual required Resource Icon { get; set; }
    public virtual required ICollection<Channel> Channels { get; set; }
    public virtual required ICollection<Account> Members { get; set; }
    public virtual required ICollection<Role> Roles { get; set; }
    public virtual required Account Owner { get; set; }
    public virtual required GroupSettings Settings { get; set; }
}