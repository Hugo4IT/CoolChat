namespace CoolChat.Domain.Models;

public class Message : BaseEntity
{
    public virtual required Account Author { get; set; }
    public DateTime Date { get; set; }
    public required string Content { get; set; }
    public virtual required Chat ParentChat { get; set; }
}