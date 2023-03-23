namespace CoolChat.Domain.Interfaces;

public interface IValidationResult
{
    public bool Success { get; }

    public IValid? Valid();
    public IInvalid? Invalid();

    public IValidationResult<T> As<T>() where T : notnull;
}

public interface IValidationResult<T> where T : notnull
{
    public bool Success { get; }

    public IValid<T>? Valid();
    public IInvalid<T>? Invalid();

    public IValidationResult Downgrade();
}