using CoolChat.Domain.Models;

namespace CoolChat.Domain.Interfaces;

public interface IGroupService
{
    public Group CreateGroup(string name, Resource icon);
    public void AddMember(Group group, Account account);
    public void AddMembers(Group group, params Account[] accounts);
    public Group? GetById(int id);
}