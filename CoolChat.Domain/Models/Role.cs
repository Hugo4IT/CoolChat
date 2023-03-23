namespace CoolChat.Domain.Models;

public class Role : BaseEntity
{
    public required string Name { get; set; }
    public required string Color { get; set; }

    public virtual required Group Group { get; set; }
    public virtual required ICollection<Account> Accounts { get; set; }

    public virtual required RolePermissions Permissions { get; set; }
}