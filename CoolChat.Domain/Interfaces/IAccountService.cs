using CoolChat.Domain.Models;

namespace CoolChat.Domain.Interfaces;

public interface IAccountService
{
    public Task<IValidationResult<Account>> CreateAccountAsync(string username, string password);
    public Task<IValidationResult<Account>> LoginAsync(string username, string password);
    public Task<Account?> GetByUsernameAsync(string username);

    public Task<IEnumerable<Group>> GetGroupsAsync(int accountId);
}