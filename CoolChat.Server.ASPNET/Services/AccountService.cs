using System.Net;
using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;
using static CoolChat.Server.ASPNET.Validation;

namespace CoolChat.Server.ASPNET.Services;

public class AccountService : IAccountService
{
    private readonly DataContext _dataContext;
    private readonly ILogger<AccountService> _logger;

    public AccountService(DataContext dataContext, ILogger<AccountService> logger)
    {
        _dataContext = dataContext;
        _logger = logger;
    }

    public async Task<IValidationResult<Account>> CreateAccountAsync(string username, string password)
    {
        username = WebUtility.HtmlEncode(username).Trim();

        ValidationBuilder validation = new();

        validation.Guard(nameof(username),
            (() => string.IsNullOrWhiteSpace(username), "Can't have an empty username"),
            (() => username.Length > 56, "Your username may not contain more than 56 characters"),
            (() => _dataContext.Accounts.Any(a => a.Name == username), "An account with this name already exists!"));

        validation.Guard(nameof(password),
            (() => password.Length < 12, "Your password must contain at least 12 characters"),
            (() => password.Length > 56, "Your password may not contain more than 56 characters"));

        if (validation.Build() is IInvalid invalid)
            return invalid.As<Account>();

        var hashedPassword = BC.EnhancedHashPassword(password);

        var account = new Account
        {
            Name = username,
            Password = hashedPassword,
            Email = "",
            Messages = new List<Message>(),
            Groups = new List<Group>(),
            Profile = new Profile(),
            Settings = new Settings(),
            Roles = new List<Role>(),
            SentInvites = new List<Invite>(),
            ReceivedInvites = new List<Invite>(),
            WebPushSubscriptions = new List<WebPushSubscription>(),

            RefreshTokenExpiryTime = DateTime.Now
        };

        await _dataContext.Accounts.AddAsync(account);
        await _dataContext.SaveChangesAsync();

        _logger.LogInformation($"Account created with name \"{account.Name.ToSafe()}\"");

        return Valid(account);
    }

    public async Task<IValidationResult<Account>> LoginAsync(string username, string password)
    {
        username = WebUtility.HtmlEncode(username).Trim();

        ValidationBuilder validation = new();

        validation.Guard(nameof(username),
            (() => string.IsNullOrWhiteSpace(username), "Please enter a username"));

        var account = await GetByUsernameAsync(username);

        validation.Guard(nameof(username),
            (() => account == null, $"No user exists with the name {username}"));

        if (validation.Build() is IInvalid invalid)
            return invalid.As<Account>();

        if (BC.EnhancedVerify(password, account!.Password))
        {
            _logger.LogTrace($"\"{account.Name.ToSafe()}\" logged in");
            return Valid(account!);
        }

        _logger.LogTrace($"\"{account.Name.ToSafe()}\" logged in");

        return Invalid<Account>(nameof(password), "Invalid password");
    }

    public Task<Account?> GetByUsernameAsync(string username)
    {
        return _dataContext.Accounts.FirstOrDefaultAsync(a => a.Name == username);
    }

    public async Task<IEnumerable<Group>> GetGroupsAsync(int accountId) =>
        await _dataContext
            .Groups
            .Include(g => g.Channels)
            .ThenInclude(c => c.Chat)
            .Include(g => g.Members)
            .Include(g => g.Icon)
            .Where(g => g.Members.Any(a => a.Id == accountId))
            .ToListAsync();
}