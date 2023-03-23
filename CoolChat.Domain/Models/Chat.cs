namespace CoolChat.Domain.Models;

public class Chat : BaseEntity
{
    public virtual required ICollection<Message> Messages { get; set; }
    public virtual required ICollection<Account> Members { get; set; }
}