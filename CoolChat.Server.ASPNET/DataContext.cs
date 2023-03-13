using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CoolChat.Domain.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoolChat.Server.ASPNET;

public class DataContext : DbContext
{
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

    public DataContext() {}

    public DataContext(DbContextOptions options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        EntityTypeBuilder<Account> account = builder.Entity<Account>();
        account.HasOne(a => a.Profile);
        account.HasOne(a => a.Settings);
        account.HasMany(a => a.Messages).WithOne(m => m.Author);
        account.HasMany(a => a.Chats).WithMany(c => c.Members);
        account.HasMany(a => a.Roles).WithMany(r => r.Accounts);
        account.HasMany(a => a.SentInvites).WithOne(i => i.From);
        account.HasMany(a => a.ReceivedInvites).WithOne(i => i.To);
        
        EntityTypeBuilder<Group> group = builder.Entity<Group>();
        group.HasMany(g => g.Channels);
        group.HasMany(g => g.Members).WithMany(a => a.Groups);
        group.HasOne(g => g.Icon);
        group.HasMany(g => g.Roles).WithOne(r => r.Group);
        group.HasOne(g => g.Owner);
        group.HasOne(g => g.Settings);

        EntityTypeBuilder<Channel> channel = builder.Entity<Channel>();
        channel.HasOne(c => c.Chat);
        channel.HasMany(c => c.AllowedRoles);

        EntityTypeBuilder<Chat> chat = builder.Entity<Chat>();
        chat.HasMany(c => c.Messages).WithOne(m => m.ParentChat);
        chat.HasMany(c => c.Members).WithMany(m => m.Chats);

        EntityTypeBuilder<Message> message = builder.Entity<Message>();
        message.HasOne(m => m.ParentChat).WithMany(c => c.Messages);
        message.HasOne(m => m.Author).WithMany(a => a.Messages);

        EntityTypeBuilder<Profile> profile = builder.Entity<Profile>();
        
        EntityTypeBuilder<Settings> settings = builder.Entity<Settings>();

        EntityTypeBuilder<GroupSettings> groupSettings = builder.Entity<GroupSettings>();
        groupSettings.HasMany(s => s.BannedAccounts);
        groupSettings.HasMany(s => s.RolePermissions);

        EntityTypeBuilder<Resource> resource = builder.Entity<Resource>();

        EntityTypeBuilder<Role> role = builder.Entity<Role>();
        role.HasMany(r => r.Accounts).WithMany(a => a.Roles);
        role.HasOne(r => r.Group).WithMany(g => g.Roles);
        role.HasOne(r => r.Permissions);

        EntityTypeBuilder<RolePermissions> rolePermissions = builder.Entity<RolePermissions>();

        EntityTypeBuilder<Invite> invite = builder.Entity<Invite>();
        invite.HasOne(i => i.From).WithMany(a => a.SentInvites);
        invite.HasOne(i => i.To).WithMany(a => a.ReceivedInvites);
    }
}