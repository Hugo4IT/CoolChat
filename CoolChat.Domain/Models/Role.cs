namespace CoolChat.Domain.Models;

public class Role : BaseEntity
{
    public string Name { get; set; }
    public string Color { get; set; }

    public virtual Group Group { get; set; }
    public virtual ICollection<Account> Accounts { get; set; }

    public virtual RolePermissions Permissions { get; set; }
}