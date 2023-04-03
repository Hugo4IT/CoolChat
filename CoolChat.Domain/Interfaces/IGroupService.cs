using CoolChat.Domain.Models;

namespace CoolChat.Domain.Interfaces;

public interface IGroupService
{
    public Group CreateGroup(Account owner, string name, Resource icon);
    public void AddMember(Group group, Account account);
    public void AddMembers(Group group, params Account[] accounts);
    public Group? GetById(int id);
    public Task<bool> HasMemberAsync(Group group, Account account);
}