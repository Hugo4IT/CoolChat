namespace CoolChat.Domain.Interfaces;

public interface IValidationResult
{
    public IValid? Valid();
    public IInvalid? Invalid();
}

public interface IValidationResult<T> where T : notnull
{
    public IValid<T>? Valid();
    public IInvalid<T>? Invalid();
}