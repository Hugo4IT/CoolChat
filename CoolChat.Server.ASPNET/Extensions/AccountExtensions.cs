using CoolChat.Domain.Models;

namespace CoolChat.Server.ASPNET.Extensions;

public static class AccountExtensions
{
    public static bool CanInviteUserTo(this Account account, Group group)
    {
        if (!group.Members.Contains(account))
            return false;

        if (group.Settings.Public)
            return true;

        return account.Roles
            .Where(r => r.Group.Id == group.Id)
            .Any(r => r.Permissions.CanAddMembers);
    }
}