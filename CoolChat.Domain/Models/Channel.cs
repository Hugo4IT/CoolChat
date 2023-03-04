namespace CoolChat.Domain.Models;

public class Channel : BaseEntity
{
    public string Name { get; set; }
    public int Icon { get; set; }
    public virtual Chat Chat { get; set; }
}