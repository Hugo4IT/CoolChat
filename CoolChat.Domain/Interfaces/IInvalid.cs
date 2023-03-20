namespace CoolChat.Domain.Interfaces;

public interface IInvalid : IValidationResult
{
    public Dictionary<string, string> Errors { get; }
    public string? GetError(string property);

    public IInvalid<T> As<T>() where T : notnull;

    public string SafeFormattedErrors();
}

public interface IInvalid<T> : IValidationResult<T> where T : notnull
{
    public Dictionary<string, string> Errors { get; }
    public string? GetError(string property);
    
    public string SafeFormattedErrors();

}