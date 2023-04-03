using CoolChat.Domain.Models;

namespace CoolChat.Domain.Interfaces;

public interface IInviteService
{
    public Task<IValidationResult<Invite>> CreateInviteAsync(Account sender, Account? recipient, Group? group);
    public Task<IValidationResult> AcceptInviteAsync(Invite invite, Account account);
    public Task<IValidationResult> RejectInviteAsync(Invite invite, Account account);
}