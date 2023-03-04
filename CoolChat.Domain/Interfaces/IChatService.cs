using CoolChat.Domain.Models;

namespace CoolChat.Domain.Interfaces;

public interface IChatService
{
    public Chat? GetById(int id);
    public IEnumerable<Message> GetMessages(Chat chat, int start, int count);
    public void AddMessage(Chat chat, Message message);
}