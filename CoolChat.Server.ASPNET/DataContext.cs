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
    public DbSet<Resource> Resources { get; set; } = null!;

    public DataContext() {}

    public DataContext(DbContextOptions options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        EntityTypeBuilder<Account> account = builder.Entity<Account>();
        account.HasOne(a => a.Profile);
        account.HasOne(a => a.Settings);
        account.HasMany(a => a.Messages).WithOne(m => m.Author);
        account.HasMany(a => a.Chats).WithMany(c => c.Members);
        
        EntityTypeBuilder<Group> group = builder.Entity<Group>();
        group.HasMany(g => g.Channels);
        group.HasMany(g => g.Members).WithMany(a => a.Groups);
        group.HasOne(g => g.Icon);

        EntityTypeBuilder<Channel> channel = builder.Entity<Channel>();
        channel.HasOne(c => c.Chat);

        EntityTypeBuilder<Chat> chat = builder.Entity<Chat>();
        chat.HasMany(c => c.Messages).WithOne(m => m.ParentChat);
        chat.HasMany(c => c.Members).WithMany(m => m.Chats);

        EntityTypeBuilder<Message> message = builder.Entity<Message>();
        message.HasOne(m => m.ParentChat).WithMany(c => c.Messages);
        message.HasOne(m => m.Author).WithMany(a => a.Messages);

        EntityTypeBuilder<Profile> profile = builder.Entity<Profile>();
        
        EntityTypeBuilder<Settings> settings = builder.Entity<Settings>();

        EntityTypeBuilder<Resource> resources = builder.Entity<Resource>();
    }
}