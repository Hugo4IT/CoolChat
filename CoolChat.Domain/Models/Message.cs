namespace CoolChat.Domain.Models;

public class Message : BaseEntity
{
    public virtual Account Author { get; set; }
    public DateTime Date { get; set; }
    public string Content { get; set; }
    public virtual Chat ParentChat { get; set; }
}