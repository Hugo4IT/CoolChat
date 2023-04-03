using System.Net;
using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Markdig;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static CoolChat.Server.ASPNET.Validation;

namespace CoolChat.Server.ASPNET.Services;

public class ChatService : IChatService
{
    private readonly DataContext _dataContext;
    private readonly IWebPushService _webPushService;
    private readonly IHubContext<ChatHub> _chatHub;
    private readonly ILogger<ChatService> _logger;

    public ChatService(DataContext dataContext, IWebPushService webPushService, IHubContext<ChatHub> chatHub, ILogger<ChatService> logger)
    {
        _dataContext = dataContext;
        _webPushService = webPushService;
        _chatHub = chatHub;
        _logger = logger;
    }

    public Task<Chat?> GetByIdAsync(int id) =>
        _dataContext.Chats.FirstOrDefaultAsync(c => c.Id == id);

    public IAsyncEnumerable<Message> GetMessagesAsync(int id, int start, int count) =>
        _dataContext
            .Messages
            .OrderByDescending(m => m.Id)
            .Include(m => m.ParentChat)
            .Include(m => m.Author)
            .Where(m => m.ParentChat.Id == id)
            .Skip(start)
            .Take(count)
            .AsAsyncEnumerable();

    public async Task<IValidationResult> AddMessageAsync(Chat chat, Account author, string content)
    {
        if (string.IsNullOrWhiteSpace(content.Trim()))
            return Invalid("Cannot send an empty message");

        content = WebUtility.HtmlEncode(content).Trim();
        
        var pipeline = new MarkdownPipelineBuilder()
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

        var (html, plain) = await Task.Run(() =>
            (Markdown.ToHtml(content, pipeline), Markdown.ToPlainText(content, pipeline)));

        var message = new Message
        {
            Author = author,
            Content = html,
            Date = DateTime.Now,
            ParentChat = chat
        };

        await _dataContext.AddAsync(message);

        await _chatHub.Clients
            .Group($"c_{chat.Id}")
            .SendAsync("ReceiveMessage", chat.Id, message.Author.Name, message.Content, message.Date);

        // TODO: Check if user is online by making client ping every 5 minutes and add that to database as last time pinged
        await _webPushService.SendTo(
            chat.Members.Where(m =>
                m.Id != message.Author.Id && m.WebPushSubscriptions.Count > 0).ToList(),
            JsonConvert.SerializeObject(new
                {
                    type = "message",
                    data = new
                    {
                        author = message.Author.Name,
                        content = plain.Substring(0, Math.Min(300, plain.Length)).Trim(),
                        date = message.Date
                    }
                },
                new JsonSerializerSettings
                    { Error = (currentObject, context) => _logger.LogError(context.ErrorContext.Error.Message) })
        );
        
        await _dataContext.SaveChangesAsync();

        return Valid();
    }
}