namespace CoolChat.Domain.Models;

public class Chat : BaseEntity
{
    public virtual ICollection<Message> Messages { get; set; }
    public virtual ICollection<Account> Members { get; set; }
}