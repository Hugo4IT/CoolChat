using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using static CoolChat.Server.ASPNET.Validation;

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

    public async Task AddMemberAsync(Group group, Account account)
    {
        if (group.Settings.BannedAccounts.Contains(account))
        {
            _logger.LogInformation("Blacklisted user \"{accountName}\" was blocked from joining \"{groupName}\"",
                account.Name.ToSafe(), group.Name.ToSafe());
            return;
        }

        group.Members.Add(account);

        await UpdateGroupChatMembers(group);
        await _dataContext.SaveChangesAsync();

        _logger.LogInformation($"\"{account.Name.ToSafe()}\" joined \"{group.Name.ToSafe()}\"");
    }

    public async Task<IValidationResult<Channel>> AddChannelAsync(Group group, Account initiator, string name, int icon,
        bool isPrivate)
    {
        var isOwner = group.Owner.Id == initiator.Id;

        List<Role> allowedRoles = new();

        if (!isOwner)
        {
            var roleWithPermission = await _dataContext.Accounts
                .Include(a => a.Roles)
                .ThenInclude(r => r.Group)
                .Include(a => a.Roles)
                .ThenInclude(r => r.Permissions)
                .Where(a => a.Roles.Any(r => r.Group.Id == group.Id) && a.Id == initiator.Id)
                .SelectMany(a => a.Roles)
                .FirstOrDefaultAsync(r => r.Permissions.CanAddChannels);

            if (roleWithPermission == null)
                return Invalid<Channel>("Insufficient permissions!");

            if (!isPrivate)
                allowedRoles.Add(roleWithPermission);
        }
    
        var channel = new Channel
        {
            Chat = new Chat
            {
                Messages = new List<Message>(),
                Members = group.Members
            },
            Name = name,
            Icon = icon,
            IsRestricted = isPrivate,
            AllowedRoles = new List<Role>(),
        };

        group.Channels.Add(channel);
        await _dataContext.SaveChangesAsync();

        return Valid(channel);
    }

    public async Task AddMembersAsync(Group group, params Account[] accounts)
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

        await UpdateGroupChatMembers(group);
        await _dataContext.SaveChangesAsync();

        // [Debug]: ["Account1", "Account2", "Account3"] joined "Group"
        _logger.LogInformation(
            $"[{string.Join(", ", accounts.Select(account => $"\"{account.Name.ToSafe()}\""))}] joined \"{group.Name.ToSafe()}\"");
    }

    public async Task<Group> CreateGroupAsync(Account owner, string name, Resource icon)
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

        await _dataContext.AddAsync(group);
        await _dataContext.SaveChangesAsync();

        // GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

        _logger.LogInformation($"Created group \"{group.Name.ToSafe()}\"");

        return group;
    }

    public Task<Group?> GetByIdAsync(int id) => _dataContext.Groups.FirstOrDefaultAsync(g => g.Id == id);

    public Task<bool> HasMemberAsync(Group group, Account account) => Task.FromResult(group.Members.Contains(account));

    private async Task UpdateGroupChatMembers(Group group)
    {
        var groupWithData = await _dataContext.Groups
            .Include(g => g.Members)
            .Include(g => g.Channels)
            .ThenInclude(c => c.Chat)
            .FirstAsync(g => g.Id == group.Id);
        foreach (var chat in groupWithData.Channels.Select(c => c.Chat))
            chat.Members = group.Members;
    }
}