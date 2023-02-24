namespace CoolChat.Domain.Models;

public class Account : BaseEntity
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public Profile Profile { get; set; }
    public ICollection<Message> Messages { get; set; }
    public ICollection<Group> Groups { get; set; }
    public Settings Settings { get; set; }
}