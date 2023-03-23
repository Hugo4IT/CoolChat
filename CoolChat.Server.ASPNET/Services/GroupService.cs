using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace CoolChat.Server.ASPNET.Services;

public class GroupService : IGroupService
{
    private readonly IHubContext<ChatHub> _chatHub;
    private readonly DataContext _dataContext;
    private readonly ILogger<GroupService> _logger;

    public GroupService(DataContext dataContext, ILogger<GroupService> logger, IHubContext<ChatHub> chatHub)
    {
        _dataContext = dataContext;
        _logger = logger;
        _chatHub = chatHub;
    }

    public void AddMember(Group group, Account account)
    {
        if (group.Settings.BannedAccounts.Contains(account))
        {
            _logger.LogInformation("Blacklisted user \"{accountName}\" was blocked from joining \"{groupName}\"", account.Name.ToSafe(), group.Name.ToSafe());
            return;
        }

        group.Members.Add(account);

        UpdateGroupChatMembers(group);

        _dataContext.SaveChanges();

        _logger.LogInformation($"\"{account.Name.ToSafe()}\" joined \"{group.Name.ToSafe()}\"");
    }

    public void AddMembers(Group group, params Account[] accounts)
    {
        foreach (var account in accounts)
        {
            if (group.Settings.BannedAccounts.Contains(account))
            {
                _logger.LogInformation(
                    $"Blacklisted user \"{account.Name.ToSafe()}\" was blocked from joining \"{group.Name.ToSafe()}\"");
                continue;
            }

            group.Members.Add(account);
        }

        UpdateGroupChatMembers(group);

        _dataContext.SaveChanges();

        // [Debug]: ["Account1", "Account2", "Account3"] joined "Group"
        _logger.LogInformation(
            $"[{string.Join(", ", accounts.Select(account => $"\"{account.Name.ToSafe()}\""))}] joined \"{group.Name.ToSafe()}\"");
    }

    public Group CreateGroup(Account owner, string name, Resource icon)
    {
        ICollection<Account> members = new List<Account> { owner };

        var group = new Group
        {
            Name = name,
            Icon = icon,
            Members = members,
            Channels = new List<Channel>
            {
                new()
                {
                    Chat = new Chat
                    {
                        Messages = new List<Message>(),
                        Members = members
                    },
                    Name = "General",
                    Icon = 0,
                    IsRestricted = false,
                    AllowedRoles = new List<Role>()
                }
            },
            Roles = new List<Role>(),
            Settings = new GroupSettings(),
            Owner = owner
        };

        _dataContext.Add(group);
        _dataContext.SaveChanges();

        // GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

        _logger.LogInformation($"Created group \"{group.Name.ToSafe()}\"");

        return group;
    }

    public Group? GetById(int id)
    {
        return _dataContext.Groups.FirstOrDefault(g => g.Id == id);
    }

    private void UpdateGroupChatMembers(Group group)
    {
        foreach (var chat in group.Channels.Select(c => c.Chat))
            chat.Members = group.Members;
    }
}