using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using CoolChat.Server.ASPNET.Controllers;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using MD = Markdig.Markdown;
using static CoolChat.Server.ASPNET.Validation;

namespace CoolChat.Server.ASPNET;

[Authorize]
public class ChatHub : Hub
{
    private static readonly ConnectionMapping<string> Connections = new();
    private readonly IAccountService _accountService;
    private readonly IChatService _chatService;
    private readonly DataContext _dataContext;
    private readonly IGroupService _groupService;
    private readonly IInviteService _inviteService;
    private readonly ILogger<ChatHub> _logger;

    private readonly IWebPushService _webPushService;

    public ChatHub(IAccountService accountService, IWebPushService webPushService, IInviteService inviteService,
        IGroupService groupService, IChatService chatService, DataContext dataContext, ILogger<ChatHub> logger)
    {
        _accountService = accountService;
        _webPushService = webPushService;
        _inviteService = inviteService;
        _groupService = groupService;
        _chatService = chatService;
        _dataContext = dataContext;
        _logger = logger;
    }

    private async Task SubscribeToGroup(string username, Group group)
    {
        var connections = Connections.GetConnections(username).ToList();
        var chatIds = group.Channels.ToList().Select(c => c.Id).ToList();

        // Add all connections of this user to all the group's channels
        await Task.WhenAll(connections.SelectMany(connection =>
            chatIds.Select(chatId => Groups.AddToGroupAsync(connection, $"c_{chatId}"))));
    }

    private async Task UnSubscribeFromGroup(string username, Group group)
    {
        var connections = Connections.GetConnections(username).ToList();
        var chatIds = group.Channels.ToList().Select(c => c.Id).ToList();

        // Add all connections of this user to all the group's channels
        await Task.WhenAll(connections.SelectMany(connection =>
            chatIds.Select(chatId => Groups.RemoveFromGroupAsync(connection, $"c_{chatId}"))));
    }

    public async Task<IValidationResult> SendMessage(int chatId, string content)
    {
        var chat = await _chatService.GetByIdAsync(chatId);

        if (chat == null)
            return Invalid("Invalid chat");

        var author = (await _accountService.GetByUsernameAsync(Context.User!.Identity!.Name!))!;

        return await _chatService.AddMessageAsync(chat, author, content);
    }

    public async Task<IValidationResult> CreateInvite(int groupId, string to)
    {
        var account = (await _accountService.GetByUsernameAsync(Context.User!.Identity!.Name!))!;
        var recipient = await _accountService.GetByUsernameAsync(to);
        var group = await _groupService.GetByIdAsync(groupId);

        var result = await _inviteService.CreateInviteAsync(account, recipient, group);

        if (!result.Success)
            return result.Downgrade();

        var invite = result.Valid()!.Value;

        await Clients.Group($"u_{recipient!.Id}")
            .SendAsync("ReceiveGroupInvite", new InviteDto
            {
                InviteId = invite.Id,
                GroupId = group!.Id,
                GroupName = group.Name,
                MemberCount = group.Members.Count,
                GroupIcon = group.Icon,
                SenderName = account.Name,
                SenderId = account.Id
            });

        return Valid();
    }

    public async Task<IValidationResult> AcceptInvite(int inviteId)
    {
        var invite = _dataContext.Invites.FirstOrDefault(i => i.Id == inviteId);

        if (invite == null)
            return Invalid("Invite does not exist");

        var account = (await _accountService.GetByUsernameAsync(Context.User!.Identity!.Name!))!;

        if (await _inviteService.AcceptInviteAsync(invite, account) is IInvalid invalid)
        {
            _logger.LogWarning($"Failed to accept invite: {invalid.SafeFormattedErrors()}");
            return invalid;
        }

        var group = (await _groupService.GetByIdAsync (invite.InvitedId))!;

        await SubscribeToGroup(account.Name, group);
        await Clients.Group($"u_{account.Id}")
            .SendAsync("GroupJoined", GroupDto.FromModel(group));

        return Valid();
    }

    public async Task<IValidationResult> RejectInvite(int inviteId)
    {
        var invite = _dataContext.Invites.FirstOrDefault(i => i.Id == inviteId);

        if (invite == null)
            return Invalid("Invite does not exist");

        var account = (await _accountService.GetByUsernameAsync(Context.User!.Identity!.Name!))!;

        if (await _inviteService.RejectInviteAsync(invite, account) is IInvalid invalid)
        {
            _logger.LogWarning($"Failed to reject invite: {invalid.SafeFormattedErrors()}");
            return invalid;
        }

        return Valid();
    }

    public async Task<IValidationResult> TrySubscribeToGroup(int groupId)
    {
        var username = Context.User!.Identity!.Name!;
        var account = (await _accountService.GetByUsernameAsync(username))!;
        var group = await _groupService.GetByIdAsync(groupId);
        
        if (group == null)
            return Invalid("This group does not exist");

        if (!await _groupService.HasMemberAsync(group, account))
            return Invalid("You do not have access to this group");

        await SubscribeToGroup(username, group);
        
        return Valid();
    }
    
    public override async Task OnConnectedAsync()
    {
        var name = Context.User!.Identity!.Name!;

        Connections.Add(name, Context.ConnectionId);

        var account = (await _accountService.GetByUsernameAsync(name))!;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"u_{account.Id}");
        foreach (var group in account.Groups) await SubscribeToGroup(account.Name, group);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var name = Context.User!.Identity!.Name!;

        Connections.Remove(name, Context.ConnectionId);

        var account = (await _accountService.GetByUsernameAsync(name))!;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"u_{account.Id}");
        foreach (var group in account.Groups) await UnSubscribeFromGroup(account.Name, group);

        await base.OnDisconnectedAsync(exception);
    }
}