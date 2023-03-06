using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;

namespace CoolChat.Server.ASPNET.Services;

public class ChatService : IChatService
{
    private readonly DataContext _dataContext;

    public ChatService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }
    
    public Chat? GetById(int id) =>
        _dataContext.Chats.FirstOrDefault(c => c.Id == id);

    public IEnumerable<Message> GetMessages(Chat chat, int start, int count) =>
        chat.Messages
            .SkipLast(start)
            .TakeLast(count);
        
    public void AddMessage(Chat chat, Message message)
    {
        chat.Messages.Add(message);
        _dataContext.SaveChanges();
    }
}