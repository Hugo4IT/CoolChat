namespace CoolChat.Server.ASPNET;

public static class StringExtensions
{
    public static string ToSafe(this string str) =>
        new string(str.Select(c => c switch
        {
            char safe when ("1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(c)) => safe,
            _ => '_',
        }).ToArray());
}