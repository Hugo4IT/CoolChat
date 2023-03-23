using CoolChat.Domain.Models;

namespace CoolChat.Domain.Interfaces;

public interface IChatService
{
    public Task<Chat?> GetByIdAsync(int id);
    public IAsyncEnumerable<Message> GetMessagesAsync(int id, int start, int count);
    public Task<IValidationResult> AddMessageAsync(Chat chat, Account author, string content);
}