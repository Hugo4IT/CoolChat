using CoolChat.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CoolChat.Server.ASPNET;

public class DataContext : DbContext
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<Channel> Channels { get; set; } = null!;
    public DbSet<Chat> Chats { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Settings> AccountSettings { get; set; } = null!;
    public DbSet<GroupSettings> GroupSettings { get; set; } = null!;
    public DbSet<Resource> Resources { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<RolePermissions> RolePermissions { get; set; } = null!;
    public DbSet<Invite> Invites { get; set; } = null!;
    public DbSet<WebPushSubscription> WebPushSubscriptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Account>().HasOne(a => a.Profile);
        builder.Entity<Account>().HasOne(a => a.Settings);
        builder.Entity<Account>().HasMany(a => a.Messages).WithOne(m => m.Author);
        builder.Entity<Account>().HasMany(a => a.Roles).WithMany(r => r.Accounts);
        builder.Entity<Account>().HasMany(a => a.SentInvites).WithOne(i => i.From);
        builder.Entity<Account>().HasMany(a => a.ReceivedInvites).WithOne(i => i.To);
        builder.Entity<Account>().HasMany(a => a.WebPushSubscriptions);

        builder.Entity<Group>().HasMany(g => g.Channels);
        builder.Entity<Group>().HasMany(g => g.Members).WithMany(a => a.Groups);
        builder.Entity<Group>().HasOne(g => g.Icon);
        builder.Entity<Group>().HasMany(g => g.Roles).WithOne(r => r.Group);
        builder.Entity<Group>().HasOne(g => g.Owner);
        builder.Entity<Group>().HasOne(g => g.Settings);

        builder.Entity<Channel>().HasOne(c => c.Chat);
        builder.Entity<Channel>().HasMany(c => c.AllowedRoles);

        builder.Entity<Chat>().HasMany(c => c.Messages).WithOne(m => m.ParentChat);
        builder.Entity<Chat>().HasMany(c => c.Members);

        builder.Entity<Message>().HasOne(m => m.ParentChat).WithMany(c => c.Messages);
        builder.Entity<Message>().HasOne(m => m.Author).WithMany(a => a.Messages);

        builder.Entity<GroupSettings>().HasMany(s => s.BannedAccounts);
        builder.Entity<GroupSettings>().HasMany(s => s.RolePermissions);

        builder.Entity<Role>().HasMany(r => r.Accounts).WithMany(a => a.Roles);
        builder.Entity<Role>().HasOne(r => r.Group).WithMany(g => g.Roles);
        builder.Entity<Role>().HasOne(r => r.Permissions);

        builder.Entity<Invite>().HasOne(i => i.From).WithMany(a => a.SentInvites);
        builder.Entity<Invite>().HasOne(i => i.To).WithMany(a => a.ReceivedInvites);
    }
}