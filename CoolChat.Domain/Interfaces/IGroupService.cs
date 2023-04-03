using CoolChat.Domain.Models;

namespace CoolChat.Domain.Interfaces;

public interface IGroupService
{
    public Task<Group> CreateGroupAsync(Account owner, string name, Resource icon);
    public Task<IValidationResult<Channel>> AddChannelAsync(Group group, Account initiator, string name, int icon, bool isPrivate);
    public Task AddMemberAsync(Group group, Account account);
    public Task AddMembersAsync(Group group, params Account[] accounts);
    public Task<Group?> GetByIdAsync(int id);
    public Task<bool> HasMemberAsync(Group group, Account account);
}