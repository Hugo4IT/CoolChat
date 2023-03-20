using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using CoolChat.Server.ASPNET.Controllers;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

using MD = Markdig.Markdown;

namespace CoolChat.Server.ASPNET;

public class HubResponse
{
    public bool Success { get; set; }
    public Dictionary<string, string>? Errors { get; set; }

    public static HubResponse FromValidationResult(IValidationResult result) => result switch
    {
        IValid valid => new HubResponse { Success = true },
        IInvalid invalid => new HubResponse { Success = false, Errors = invalid.Errors },
        _ => throw new InvalidCastException(),
    };

    public static HubResponse FromValidationResult<T>(IValidationResult<T> result) where T : notnull => result switch
    {
        IValid<T> valid => new HubResponse { Success = true },
        IInvalid<T> invalid => new HubResponse { Success = false, Errors = invalid.Errors },
        _ => throw new InvalidCastException(),
    };

    public static HubResponse Ok() => new HubResponse { Success = true };
    public static HubResponse Fault(string error) => new HubResponse { Success = false, Errors = new Dictionary<string, string>() { ["default"] = error }};
    public static HubResponse Fault(Dictionary<string, string> errors) => new HubResponse { Success = false, Errors = errors};
}

[Authorize]
public class ChatHub : Hub
{
    private readonly static ConnectionMapping<string> _connections = new();

    private readonly IWebPushService _webPushService;
    private readonly IAccountService _accountService;
    private readonly IInviteService _inviteService;
    private readonly IGroupService _groupService;
    private readonly IChatService _chatService;
    private readonly DataContext _dataContext;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IAccountService accountService, IWebPushService webPushService, IInviteService inviteService, IGroupService groupService, IChatService chatService, DataContext dataContext, ILogger<ChatHub> logger)
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
        List<string> connections = _connections.GetConnections(username).ToList();
        List<int> chatIds = group.Channels.Select(c => c.Id).ToList();

        // Add all connections of this user to all the group's channels
        await Task.WhenAll(connections.SelectMany(connection => chatIds.Select(chatId => Groups.AddToGroupAsync(connection, $"c_{chatId}"))));
    }

    private async Task UnSubscribeFromGroup(string username, Group group)
    {
        List<string> connections = _connections.GetConnections(username).ToList();
        List<int> chatIds = group.Channels.Select(c => c.Id).ToList();

        // Add all connections of this user to all the group's channels
        await Task.WhenAll(connections.SelectMany(connection => chatIds.Select(chatId => Groups.RemoveFromGroupAsync(connection, $"c_{chatId}"))));
    }
    
    public async Task SendMessage(int chatId, string content)
    {
        Chat? chat = _chatService.GetById(chatId);
        
        if (chat == null)
            return;

        if (string.IsNullOrWhiteSpace(content.Trim()))
            return;

        MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
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

        string html = MD.ToHtml(content, pipeline);
        string plain = MD.ToPlainText(content, pipeline);

        Message message = new Message
        {
            Author = _accountService.GetByUsername(Context.User!.Identity!.Name!)!,
            Content = html,
            Date = DateTime.Now,
        };

        _chatService.AddMessage(chat, message);

        await Clients.Group($"c_{chat.Id}")
                     .SendAsync("ReceiveMessage", chatId, message.Author.Name, message.Content, message.Date);

        await _webPushService.SendTo(
            chat.Members.Where(m => m.Id != message.Author.Id && m.WebPushSubscriptions.Count > 0 && _connections.GetConnections(m.Name).Count() == 0).ToList(),
            JsonConvert.SerializeObject(new
            {
                type = "message",
                data = new
                {
                    author = message.Author.Name,
                    content = plain.Substring(0, Math.Min(300, plain.Length)).Trim(),
                    date = message.Date,
                },
            }, new JsonSerializerSettings { Error = (currentObject, context) => _logger.LogError(context.ErrorContext.Error.Message) })
        );
    }

    

    public async Task<HubResponse> CreateInvite(int groupId, string to)
    {
        Account account = _accountService.GetByUsername(Context.User!.Identity!.Name!)!;
        Account? recipient = _accountService.GetByUsername(to);
        Group? group = _groupService.GetById(groupId);

        IValidationResult<Invite> result = await _inviteService.CreateInvite(account, recipient, group);

        if (result is IInvalid<Invite> invalid)
            return HubResponse.FromValidationResult(invalid);

        Invite invite = result.Valid()!.Value;

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
                    
        return HubResponse.Ok();
    }

    public async Task<HubResponse> AcceptInvite(int inviteId)
    {
        Invite? invite = _dataContext.Invites.FirstOrDefault(i => i.Id == inviteId);

        if (invite == null)
            return HubResponse.Fault("Invite does not exist");
        
        Account account = _accountService.GetByUsername(Context.User!.Identity!.Name!)!;
        
        if (await _inviteService.AcceptInvite(invite, account) is IInvalid invalid)
        {
            _logger.LogWarning($"Failed to accept invite: {invalid.SafeFormattedErrors()}");
            return HubResponse.FromValidationResult(invalid);
        }

        Group group = _groupService.GetById(invite.InvitedId)!;

        await SubscribeToGroup(account.Name, group);
        await Clients.Group($"u_{account.Id}")
                     .SendAsync("GroupJoined", GroupDto.FromModel(group));
                    
        return HubResponse.Ok();
    }

    public async Task<HubResponse> RejectInvite(int inviteId)
    {
        Invite? invite = _dataContext.Invites.FirstOrDefault(i => i.Id == inviteId);

        if (invite == null)
            return HubResponse.Fault("Invite does not exist");

        Account account = _accountService.GetByUsername(Context.User!.Identity!.Name!)!;

        if (await _inviteService.RejectInvite(invite, account) is IInvalid invalid)
        {
            _logger.LogWarning($"Failed to reject invite: {invalid.SafeFormattedErrors()}");
            return HubResponse.FromValidationResult(invalid);
        }

        return HubResponse.Ok();
    }

    public override async Task OnConnectedAsync()
    {
        string name = Context.User!.Identity!.Name!;

        _connections.Add(name, Context.ConnectionId);

        Account account = _accountService.GetByUsername(name)!;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"u_{account.Id}");
        foreach (Group group in account.Groups)
        {
            await SubscribeToGroup(account.Name, group);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string name = Context.User!.Identity!.Name!;

        _connections.Remove(name, Context.ConnectionId);

        Account account = _accountService.GetByUsername(name)!;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"u_{account.Id}");
        foreach (Group group in account.Groups)
        {
            await UnSubscribeFromGroup(account.Name, group);
        }

        await base.OnDisconnectedAsync(exception);
    }
}