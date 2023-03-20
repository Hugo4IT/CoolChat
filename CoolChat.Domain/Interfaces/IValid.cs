namespace CoolChat.Domain.Interfaces;

public interface IValid : IValidationResult
{
}

public interface IValid<T> : IValidationResult<T> where T : notnull
{
    public T Value { get; }
}