using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using CoolChat.Server.ASPNET.Extensions;
using Microsoft.AspNetCore.SignalR;
using static CoolChat.Server.ASPNET.Validation;

namespace CoolChat.Server.ASPNET.Services;

public class InviteService : IInviteService
{
    private readonly DataContext _dataContext;
    private readonly IGroupService _groupService;
    private readonly IHubContext<ChatHub> _hubContext;

    public InviteService(DataContext dataContext, IGroupService groupService, IHubContext<ChatHub> hubContext)
    {
        _groupService = groupService;
        _dataContext = dataContext;
        _hubContext = hubContext;
    }

    public async Task<IValidationResult<Invite>> CreateInviteAsync(Account sender, Account? recipient, Group? group)
    {
        ValidationBuilder validation = new();

        validation.Guard(nameof(recipient),
            (() => recipient == null, "This account doesn't exist"),
            (() => sender.Id == recipient?.Id, "Can't send an invite to yourself, dummy"));

        validation.Guard(nameof(group),
            (() => group == null, "You're trying to invite this person to a group that doesn't exist"),
            (() => !sender.CanInviteUserTo(group!), "You aren't allowed to invite people to this group"));

        if (validation.Build() is IInvalid invalid)
            return invalid.As<Invite>();

        var invite = new Invite
        {
            From = sender,
            To = recipient!,
            Type = InviteType.Group,
            InvitedId = group!.Id,
            Expires = DateTime.Now.AddDays(7)
        };

        _dataContext.Invites.Add(invite);
        await _dataContext.SaveChangesAsync();

        return Valid(invite);
    }

    public async Task<IValidationResult> AcceptInviteAsync(Invite invite, Account account)
    {
        var group = await _groupService.GetByIdAsync(invite.InvitedId);

        ValidationBuilder validation = new();

        validation.Guard(nameof(group),
            (() => group == null, "The group you've been invited to doesn't exist anymore"));

        validation.Guard(nameof(invite),
            (() => invite.Expires < DateTime.Now, "This invite has expired"),
            (() => group != null && !invite.From.CanInviteUserTo(group!),
                "The account which sent you this invite isn't allowed to send invites to this server"));

        validation.Guard(nameof(account),
            (() => account.Id != invite.To.Id, "This invite is not for you"));

        var validationResult = validation.Build();

        if (validationResult.Success)
            await _groupService.AddMemberAsync(group!, account);

        _dataContext.Invites.Remove(invite);
        await _dataContext.SaveChangesAsync();

        return validationResult;
    }

    public async Task<IValidationResult> RejectInviteAsync(Invite invite, Account account)
    {
        ValidationBuilder validation = new();
        validation.Guard(nameof(invite), (() => account.Id != invite.To!.Id, "This invite is not for you"));

        if (validation.Build() is IInvalid invalid)
            return invalid;

        _dataContext.Invites.Remove(invite);
        await _dataContext.SaveChangesAsync();

        return Valid();
    }
}