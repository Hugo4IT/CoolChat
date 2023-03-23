namespace CoolChat.Domain.Models;

public class Channel : BaseEntity
{
    public required string Name { get; set; }
    public required int Icon { get; set; }
    public virtual required Chat Chat { get; set; }
    public required bool IsRestricted { get; set; }
    public virtual required ICollection<Role> AllowedRoles { get; set; }
}