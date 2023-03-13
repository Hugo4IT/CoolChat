using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using CoolChat.Domain.Interfaces;
using CoolChat.Domain.Models;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

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

    public LoginResult CreateAccount(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return new LoginResult
            {
                Success = false,
                UsernameError = "Enter a username",
            };
        }

        if (password.Length < 12 || password.Length > 56)
        {
            return new LoginResult
            {
                Success = false,
                PasswordError = "Password cannot be longer than 56 characters and must be longer than 11",
            };
        }

        if (_dataContext.Accounts.Any(account => account.Name == username))
        {
            return new LoginResult
            {
                Success = false,
                UsernameError = "Username unavailable",
            };
        }
        
        string hashedPassword = BC.EnhancedHashPassword(password);

        Account account = new Account
        {
            Name = username,
            Password = hashedPassword,
            Email = "",
            Messages = new List<Message>(),
            Profile = new(),
            Settings = new(),
            Roles = new List<Role>(),

            RefreshTokenExpiryTime = DateTime.Now,
        };

        _dataContext.Accounts.Add(account);
        _dataContext.SaveChanges();

        _logger.LogInformation($"Account created with name \"{account.Name.ToSafe()}\"");

        return new LoginResult
        {
            Success = true,
            Account = account,
        };
    }

    public LoginResult Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return new LoginResult
            {
                Success = false,
                UsernameError = "Enter a username",
            };
        }

        Account? account = _dataContext.Accounts.FirstOrDefault(account => account.Name == username);

        if (account is null)
        {
            return new LoginResult
            {
                Success = false,
                UsernameError = $"No user exists with the name {username}",
            };
        }
        
        if (BC.EnhancedVerify(password, account.Password))
        {
            return new LoginResult
            {
                Success = true,
                Account = account,
            };
        }

        _logger.LogTrace($"\"{account.Name.ToSafe()}\" logged in");
        
        return new LoginResult
        {
            Success = false,
            PasswordError = "Incorrect password",
        };
    }

    public Account? GetByUsername(string username) =>
        _dataContext.Accounts.FirstOrDefault(a => a.Name == username);
}