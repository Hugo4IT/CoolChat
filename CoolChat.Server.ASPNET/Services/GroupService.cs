using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CoolChat.Server.ASPNET.Services;

public class GroupService : IGroupService
{
    private readonly DataContext _dataContext;
    
    public GroupService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public void AddMember(Group group, Account account)
    {
        group.Members.Add(account);
        _dataContext.SaveChanges();
    }

    public void AddMembers(Group group, params Account[] accounts)
    {
        foreach (Account account in accounts)
            group.Members.Add(account);
        
        _dataContext.SaveChanges();
    }

    public Group CreateGroup(string name, Resource icon)
    {
        ICollection<Account> members = new List<Account>();

        Group group = new Group
        {
            Name = name,
            Icon = icon,
            Members = members,
            Channels = new List<Channel>()
            {
                new Channel
                {
                    Chat = new Chat
                    {
                        Messages = new List<Message>(),
                        Members = members,
                    },
                    Name = "General",
                    Icon = 0,
                }
            },
        };

        _dataContext.Add(group);
        _dataContext.SaveChanges();

        return group;
    }

    public Group? GetById(int id) =>
        _dataContext.Groups.FirstOrDefault(g => g.Id == id);
}
