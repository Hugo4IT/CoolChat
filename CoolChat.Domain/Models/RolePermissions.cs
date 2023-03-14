namespace CoolChat.Domain.Models;

public class RolePermissions : BaseEntity
{
    public string Name { get; set; }

    public bool CanEditOtherUsersMessages { get; set; } = false;
    public bool CanAddMembers { get; set; } = false;
}