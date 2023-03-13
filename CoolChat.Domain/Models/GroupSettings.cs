namespace CoolChat.Domain.Models;

public class GroupSettings : BaseEntity
{
    public string PrimaryColor { get; set; } = "blue";
    public bool Public { get; set; } = true;
    public virtual ICollection<Account> BannedAccounts { get; set; } = new List<Account>();
    public virtual ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
}