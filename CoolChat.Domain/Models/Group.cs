namespace CoolChat.Domain.Models;

public class Group : BaseEntity
{
    public string Name { get; set; }
    public virtual Resource Icon { get; set; }
    public virtual ICollection<Channel> Channels { get; set; }
    public virtual ICollection<Account> Members { get; set; }
    public virtual ICollection<Role> Roles { get; set; }
    public virtual Account Owner { get; set; }
    public virtual GroupSettings Settings { get; set; }
}