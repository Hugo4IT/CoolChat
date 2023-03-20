using CoolChat.Domain.Interfaces;

namespace CoolChat.Server.ASPNET;

public class Valid : IValid
{
    IValid? IValidationResult.Valid() => this;
    IInvalid? IValidationResult.Invalid() => null;
}

public class Invalid : IInvalid
{
    public Dictionary<string, string> Errors { get; set; }

    internal Invalid(Dictionary<string, string> errors) => Errors = errors;

    public string? GetError(string property)
    {
        if (Errors.TryGetValue(property, out string? result))
            return result;
        return null;
    }

    IValid? IValidationResult.Valid() => null;
    IInvalid? IValidationResult.Invalid() => this;

    public IInvalid<T> As<T>() where T : notnull => new Invalid<T>(Errors);

    public string SafeFormattedErrors() =>
        $"{{{string.Join(", ", Errors.Select(e => $"'{e.Key.ToSafe()}': '{e.Value.ToSafe()}'"))}}}";
}

public class Valid<T> : IValid<T> where T : notnull
{
    public T Value { get; }

    internal Valid(T value) => Value = value;

    IValid<T>? IValidationResult<T>.Valid() => this;
    IInvalid<T>? IValidationResult<T>.Invalid() => null;
}

public class Invalid<T> : IInvalid<T> where T : notnull
{
    public Dictionary<string, string> Errors { get; set; }

    internal Invalid(Dictionary<string, string> errors) => Errors = errors;

    public string? GetError(string property)
    {
        if (Errors.TryGetValue(property, out string? result))
            return result;
        return null;
    }

    IValid<T>? IValidationResult<T>.Valid() => null;
    IInvalid<T>? IValidationResult<T>.Invalid() => this;

    public string SafeFormattedErrors() =>
        $"{{{string.Join(", ", Errors.Select(e => $"'{e.Key.ToSafe()}': '{e.Value.ToSafe()}'"))}}}";
}

public static class Validation
{
    public static IInvalid? Guard(string property, params (Func<bool> test, string error)[] tests)
    {
        if (tests.Where(t => t.test()).Select(t => t.error).FirstOrDefault() is string error)
            return new Invalid(new Dictionary<string, string>() { [property] = error });
        
        return null;
    }

    public static IInvalid<T>? Guard<T>(string property, params (Func<bool> test, string error)[] tests) where T : notnull
    {
        if (tests.Where(t => t.test()).Select(t => t.error).FirstOrDefault() is string error)
            return new Invalid<T>(new Dictionary<string, string>() { [property] = error });
        
        return null;
    }

    public static IValidationResult<T> Valid<T>(T value) where T : notnull => new Valid<T>(value);
    public static IValidationResult Valid() => new Valid();
}