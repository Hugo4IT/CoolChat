﻿// <auto-generated />
using System;
using CoolChat.Server.ASPNET;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CoolChat.Server.ASPNET.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20230317085450_WebPushSubscriptions")]
    partial class WebPushSubscriptions
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("AccountGroup", b =>
                {
                    b.Property<int>("GroupsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MembersId")
                        .HasColumnType("INTEGER");

                    b.HasKey("GroupsId", "MembersId");

                    b.HasIndex("MembersId");

                    b.ToTable("AccountGroup");
                });

            modelBuilder.Entity("AccountRole", b =>
                {
                    b.Property<int>("AccountsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RolesId")
                        .HasColumnType("INTEGER");

                    b.HasKey("AccountsId", "RolesId");

                    b.HasIndex("RolesId");

                    b.ToTable("AccountRole");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("GroupSettingsId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ProfileId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("RefreshTokenExpiryTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("SettingsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ChatId");

                    b.HasIndex("GroupSettingsId");

                    b.HasIndex("ProfileId");

                    b.HasIndex("SettingsId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Channel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChatId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Icon")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsRestricted")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ChatId");

                    b.HasIndex("GroupId");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Chat", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Chats");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Group", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("IconId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SettingsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("IconId");

                    b.HasIndex("OwnerId");

                    b.HasIndex("SettingsId");

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.GroupSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("PrimaryColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Public")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("GroupSettings");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Invite", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Expires")
                        .HasColumnType("TEXT");

                    b.Property<int>("FromId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("InvitedId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ToId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("FromId");

                    b.HasIndex("ToId");

                    b.ToTable("Invites");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<int>("ParentChatId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("ParentChatId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Profile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Resource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("OriginalFileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Resources");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("GroupId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("PermissionsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ChannelId");

                    b.HasIndex("GroupId");

                    b.HasIndex("PermissionsId");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.RolePermissions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("CanAddMembers")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("CanEditOtherUsersMessages")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("GroupSettingsId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GroupSettingsId");

                    b.ToTable("RolePermissions");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Settings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("AccountSettings");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.WebPushSubscription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Endpoint")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Key_auth")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Key_p256dh")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("WebPushSubscriptions");
                });

            modelBuilder.Entity("AccountGroup", b =>
                {
                    b.HasOne("CoolChat.Domain.Models.Group", null)
                        .WithMany()
                        .HasForeignKey("GroupsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoolChat.Domain.Models.Account", null)
                        .WithMany()
                        .HasForeignKey("MembersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AccountRole", b =>
                {
                    b.HasOne("CoolChat.Domain.Models.Account", null)
                        .WithMany()
                        .HasForeignKey("AccountsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoolChat.Domain.Models.Role", null)
                        .WithMany()
                        .HasForeignKey("RolesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Account", b =>
                {
                    b.HasOne("CoolChat.Domain.Models.Chat", null)
                        .WithMany("Members")
                        .HasForeignKey("ChatId");

                    b.HasOne("CoolChat.Domain.Models.GroupSettings", null)
                        .WithMany("BannedAccounts")
                        .HasForeignKey("GroupSettingsId");

                    b.HasOne("CoolChat.Domain.Models.Profile", "Profile")
                        .WithMany()
                        .HasForeignKey("ProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoolChat.Domain.Models.Settings", "Settings")
                        .WithMany()
                        .HasForeignKey("SettingsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Profile");

                    b.Navigation("Settings");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Channel", b =>
                {
                    b.HasOne("CoolChat.Domain.Models.Chat", "Chat")
                        .WithMany()
                        .HasForeignKey("ChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoolChat.Domain.Models.Group", null)
                        .WithMany("Channels")
                        .HasForeignKey("GroupId");

                    b.Navigation("Chat");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Group", b =>
                {
                    b.HasOne("CoolChat.Domain.Models.Resource", "Icon")
                        .WithMany()
                        .HasForeignKey("IconId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoolChat.Domain.Models.Account", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoolChat.Domain.Models.GroupSettings", "Settings")
                        .WithMany()
                        .HasForeignKey("SettingsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Icon");

                    b.Navigation("Owner");

                    b.Navigation("Settings");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Invite", b =>
                {
                    b.HasOne("CoolChat.Domain.Models.Account", "From")
                        .WithMany("SentInvites")
                        .HasForeignKey("FromId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoolChat.Domain.Models.Account", "To")
                        .WithMany("ReceivedInvites")
                        .HasForeignKey("ToId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("From");

                    b.Navigation("To");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Message", b =>
                {
                    b.HasOne("CoolChat.Domain.Models.Account", "Author")
                        .WithMany("Messages")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoolChat.Domain.Models.Chat", "ParentChat")
                        .WithMany("Messages")
                        .HasForeignKey("ParentChatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("ParentChat");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Role", b =>
                {
                    b.HasOne("CoolChat.Domain.Models.Channel", null)
                        .WithMany("AllowedRoles")
                        .HasForeignKey("ChannelId");

                    b.HasOne("CoolChat.Domain.Models.Group", "Group")
                        .WithMany("Roles")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CoolChat.Domain.Models.RolePermissions", "Permissions")
                        .WithMany()
                        .HasForeignKey("PermissionsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Permissions");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.RolePermissions", b =>
                {
                    b.HasOne("CoolChat.Domain.Models.GroupSettings", null)
                        .WithMany("RolePermissions")
                        .HasForeignKey("GroupSettingsId");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.WebPushSubscription", b =>
                {
                    b.HasOne("CoolChat.Domain.Models.Account", null)
                        .WithMany("WebPushSubscriptions")
                        .HasForeignKey("AccountId");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Account", b =>
                {
                    b.Navigation("Messages");

                    b.Navigation("ReceivedInvites");

                    b.Navigation("SentInvites");

                    b.Navigation("WebPushSubscriptions");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Channel", b =>
                {
                    b.Navigation("AllowedRoles");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Chat", b =>
                {
                    b.Navigation("Members");

                    b.Navigation("Messages");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.Group", b =>
                {
                    b.Navigation("Channels");

                    b.Navigation("Roles");
                });

            modelBuilder.Entity("CoolChat.Domain.Models.GroupSettings", b =>
                {
                    b.Navigation("BannedAccounts");

                    b.Navigation("RolePermissions");
                });
#pragma warning restore 612, 618
        }
    }
}
