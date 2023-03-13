namespace CoolChat.Domain.Models;

public class Channel : BaseEntity
{
    public string Name { get; set; }
    public int Icon { get; set; }
    public virtual Chat Chat { get; set; }
    public bool IsRestricted { get; set; }
    public virtual ICollection<Role> AllowedRoles { get; set; }
}