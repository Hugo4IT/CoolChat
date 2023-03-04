using CoolChat.Domain.Models;

namespace CoolChat.Domain.Interfaces;

public struct LoginResult
{
    public bool Success;
    public string? UsernameError { get; set; }
    public string? PasswordError { get; set; }

    public Account? Account { get; set; }
}

public interface IAccountService
{
    public LoginResult CreateAccount(string username, string password);
    public LoginResult Login(string username, string password);
    public Account? GetByUsername(string username);
}