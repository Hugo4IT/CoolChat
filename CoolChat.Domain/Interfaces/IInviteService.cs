using CoolChat.Domain.Models;

namespace CoolChat.Domain.Interfaces;

public interface IInviteService
{
    public Task<IValidationResult<Invite>> CreateInvite(Account sender, Account? recipient, Group? group);
    public Task<IValidationResult> AcceptInvite(Invite invite, Account account);
    public Task<IValidationResult> RejectInvite(Invite invite, Account account);
}