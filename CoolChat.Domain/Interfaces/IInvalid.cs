namespace CoolChat.Domain.Interfaces;

public interface IInvalid : IValidationResult
{
    public Dictionary<string, string> Errors { get; }
    public string? GetError(string property);

    public string SafeFormattedErrors();
}

public interface IInvalid<T> : IValidationResult<T> where T : notnull
{
    public Dictionary<string, string> Errors { get; }
    public string? GetError(string property);

    public string SafeFormattedErrors();
}