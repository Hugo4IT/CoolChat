using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using CoolChat.Server.ASPNET.Controllers;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using MD = Markdig.Markdown;

namespace CoolChat.Server.ASPNET;

[Authorize]
public class ChatHub : Hub
{
    private readonly static ConnectionMapping<string> _connections = new();

    private readonly IAccountService _accountService;
    private readonly IGroupService _groupService;
    private readonly IChatService _chatService;
    private readonly DataContext _dataContext;

    public ChatHub(IAccountService accountService, IGroupService groupService, IChatService chatService, DataContext dataContext)
    {
        _accountService = accountService;
        _groupService = groupService;
        _chatService = chatService;
        _dataContext = dataContext;
    }

    private Task Join(int chatId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    
    private Task Leave(int chatId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
    
    private async Task SubscribeToGroup(string username, Group group)
    {
        List<string> connections = _connections.GetConnections(username).ToList();
        List<int> chatIds = group.Channels.Select(c => c.Id).ToList();

        // Add all connections of this user to all the group's channels
        await Task.WhenAll(connections.SelectMany(connection => chatIds.Select(chatId => Groups.AddToGroupAsync(connection, chatId.ToString()))));
    }
    
    public async Task SendMessage(int chatId, string content)
    {
        Chat? chat = _chatService.GetById(chatId);
        
        if (chat == null)
            return;

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
            .DisableHeadings()
            .DisableHtml()
            .UseSmartyPants()
            .UseTaskLists()
            .UseListExtras()
            .UseEmphasisExtras()
            .UseAutoIdentifiers()
            .UseDefinitionLists()
            .UseEmojiAndSmiley()
            .UsePipeTables()
            .UseGridTables()
            .Build();

        content = MD.ToHtml(content, pipeline).Trim();

        if (content.Length == 0)
            return;

        Message message = new Message
        {
            Author = _accountService.GetByUsername(Context.User!.Identity!.Name!)!,
            Content = content,
            Date = DateTime.Now,
        };

        _chatService.AddMessage(chat, message);

        await Clients.Group(chat.Id.ToString()).SendAsync("ReceiveMessage", chatId, message.Author.Name, message.Content, message.Date);
    }

    public struct CreateInviteResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public async Task<CreateInviteResponse> CreateInvite(int groupId, string to)
    {
        Account account = _accountService.GetByUsername(Context.User!.Identity!.Name!)!;
        Account? recipient = _accountService.GetByUsername(to);
        Group? group = _groupService.GetById(groupId);

        if (recipient == null)
            return new CreateInviteResponse { Success = false, Error = "This account doesn't exist" };
        
        if (account.Id == recipient.Id)
            return new CreateInviteResponse { Success = false, Error = "Can't send an invite to yourself, dummy" };

        if (group == null)
            return new CreateInviteResponse { Success = false, Error = "You're trying to invite this person to a group that doesn't exist" };
        
        if (!account.CanInviteUserTo(group))
            return new CreateInviteResponse { Success = false, Error = "You aren't allowed to invite people to this group" };

        Invite invite = new Invite
        {
            From = account,
            To = recipient,
            Type = InviteType.Group,
            InvitedId = group.Id,
            Expires = DateTime.Now.AddDays(7),
        };

        _dataContext.Invites.Add(invite);
        await _dataContext.SaveChangesAsync();

        await Clients.Users(_connections.GetConnections(recipient.Name))
                     .SendAsync("ReceiveGroupInvite", group.Id, group.Name, group.Members.Count, group.Icon, account.Name, account.Id);
                    
        return new CreateInviteResponse { Success = true, Error = "" };
    }

    public async Task AcceptInvite(int inviteId)
    {
        Invite? invite = _dataContext.Invites.FirstOrDefault(i => i.Id == inviteId);

        if (invite == null)
            return;
        
        Group? group = _groupService.GetById(invite.InvitedId);

        if (invite.Expires < DateTime.Now || group == null || !invite.From.CanInviteUserTo(group))
        {
            _dataContext.Invites.Remove(invite);
            await _dataContext.SaveChangesAsync();
            return;
        }

        Account account = _accountService.GetByUsername(Context.User!.Identity!.Name!)!;

        if (account.Id != invite.To!.Id)
            return;

        _groupService.AddMember(group, account);

        _dataContext.Invites.Remove(invite);
        await _dataContext.SaveChangesAsync();

        await SubscribeToGroup(account.Name, group);

        await Clients.Users(_connections.GetConnections(account.Name))
                     .SendAsync("GroupJoined", GroupDto.FromModel(group));
    }

    public async Task RejectInvite(int inviteId)
    {
        Invite? invite = _dataContext.Invites.FirstOrDefault(i => i.Id == inviteId);

        if (invite == null)
            return;

        Account account = _accountService.GetByUsername(Context.User!.Identity!.Name!)!;

        if (account.Id != invite.To!.Id)
            return;

        _dataContext.Invites.Remove(invite);
        await _dataContext.SaveChangesAsync();
    }

    public override async Task OnConnectedAsync()
    {
        string name = Context.User!.Identity!.Name!;

        _connections.Add(name, Context.ConnectionId);

        Account account = _accountService.GetByUsername(name)!;

        foreach (Chat chat in account.Chats)
        {
            await Join(chat.Id);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string name = Context.User!.Identity!.Name!;

        _connections.Remove(name, Context.ConnectionId);

        Account account = _accountService.GetByUsername(name)!;

        foreach (Chat chat in account.Chats)
        {
            await Leave(chat.Id);
        }

        await base.OnDisconnectedAsync(exception);
    }
}