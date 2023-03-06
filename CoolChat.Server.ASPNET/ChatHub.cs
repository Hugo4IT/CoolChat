using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Ganss.Xss;
using Markdig;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CoolChat.Server.ASPNET;

[Authorize]
public class ChatHub : Hub
{
    private readonly static ConnectionMapping<string> _connections = new();

    private readonly IAccountService _accountService;
    private readonly IChatService _chatService;

    public ChatHub(IAccountService accountService, IChatService chatService)
    {
        _accountService = accountService;
        _chatService = chatService;
    }

    private Task Join(int chatId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    
    private Task Leave(int chatId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
    
    public async Task SendMessage(int chatId, string content)
    {
        Chat? chat = _chatService.GetById(chatId);
        
        if (chat == null)
            return;

        content = Markdown.ToHtml(new HtmlSanitizer().Sanitize(content)).Trim();

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

    public async Task TryJoin(int chatId)
    {
        Account account = _accountService.GetByUsername(Context.User!.Identity!.Name!)!;

        if (!account.Chats.Any(c => c.Id == chatId))
            return;

        await Join(chatId);
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