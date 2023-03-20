using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
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

    public async Task<IValidationResult<Invite>> CreateInvite(Account sender, Account? recipient, Group? group)
    {
        IInvalid? error = null;

        error ??= Guard(nameof(recipient),
                (() => recipient == null, "This account doesn't exist"),
                (() => sender.Id == recipient?.Id, "Can't send an invite to yourself, dummy"));

        error ??= Guard(nameof(group),
                (() => group == null, "You're trying to invite this person to a group that doesn't exist"),
                (() => !sender.CanInviteUserTo(group!), "You aren't allowed to invite people to this group"));

        if (error != null)
            return error.As<Invite>();
        
        Invite invite = new Invite
        {
            From = sender,
            To = recipient!,
            Type = InviteType.Group,
            InvitedId = group!.Id,
            Expires = DateTime.Now.AddDays(7),
        };

        _dataContext.Invites.Add(invite);
        await _dataContext.SaveChangesAsync();

        return Valid(invite);
    }

    public async Task<IValidationResult> AcceptInvite(Invite invite, Account account)
    {
        Group? group = _groupService.GetById(invite.InvitedId);

        IInvalid? error = null;
        
        error ??= Guard(nameof(group),
            (() => group == null, "The group you've been invited to doesn't exist anymore"));
        
        error ??= Guard(nameof(invite),
            (() => invite.Expires < DateTime.Now, "This invite has expired"),
            (() => !invite.From.CanInviteUserTo(group!), "The account which sent you this invite isn't allowed to send invites to this server"));

        error ??= Guard(nameof(account),
            (() => account.Id != invite.To.Id, "This invite is not for you"));

        if (error != null)
        {
            _dataContext.Invites.Remove(invite);
            await _dataContext.SaveChangesAsync();
            return error;
        }

        _groupService.AddMember(group!, account);
        _dataContext.Invites.Remove(invite);

        await _dataContext.SaveChangesAsync();

        return Valid();
    }

    public async Task<IValidationResult> RejectInvite(Invite invite, Account account)
    {
        if (Guard(nameof(invite), (() => account.Id != invite.To!.Id, "This invite is not for you")) is IInvalid invalid)
            return invalid;

        _dataContext.Invites.Remove(invite);
        await _dataContext.SaveChangesAsync();

        return Valid();
    }
}