using CoolChat.Domain.Interfaces;

namespace CoolChat.Server.ASPNET;

public class Valid : IValid
{
    public bool Success => true;

    IValid? IValidationResult.Valid()
    {
        return this;
    }

    IInvalid? IValidationResult.Invalid()
    {
        return null;
    }

    public IValidationResult<T> As<T>() where T : notnull
    {
        throw new NotSupportedException();
    }
}

public class Invalid : IInvalid
{
    internal Invalid(Dictionary<string, string> errors)
    {
        Errors = errors;
    }

    public bool Success => false;

    public Dictionary<string, string> Errors { get; set; }

    public string? GetError(string property)
    {
        if (Errors.TryGetValue(property, out var result))
            return result;
        return null;
    }

    IValid? IValidationResult.Valid()
    {
        return null;
    }

    IInvalid? IValidationResult.Invalid()
    {
        return this;
    }

    public IValidationResult<T> As<T>() where T : notnull
    {
        return new Invalid<T>(Errors);
    }

    public string SafeFormattedErrors()
    {
        return $"{{{string.Join(", ", Errors.Select(e => $"'{e.Key.ToSafe()}': '{e.Value.ToSafe()}'"))}}}";
    }
}

public class Valid<T> : IValid<T> where T : notnull
{
    internal Valid(T value)
    {
        Value = value;
    }

    public bool Success => true;

    public T Value { get; }

    public IValidationResult Downgrade()
    {
        return new Valid();
    }

    IValid<T>? IValidationResult<T>.Valid()
    {
        return this;
    }

    IInvalid<T>? IValidationResult<T>.Invalid()
    {
        return null;
    }
}

public class Invalid<T> : IInvalid<T> where T : notnull
{
    internal Invalid(Dictionary<string, string> errors)
    {
        Errors = errors;
    }

    public bool Success => false;

    public Dictionary<string, string> Errors { get; set; }

    public string? GetError(string property)
    {
        if (Errors.TryGetValue(property, out var result))
            return result;
        return null;
    }

    public IValidationResult Downgrade()
    {
        return new Invalid(Errors);
    }

    IValid<T>? IValidationResult<T>.Valid()
    {
        return null;
    }

    IInvalid<T>? IValidationResult<T>.Invalid()
    {
        return this;
    }

    public string SafeFormattedErrors()
    {
        return $"{{{string.Join(", ", Errors.Select(e => $"'{e.Key.ToSafe()}': '{e.Value.ToSafe()}'"))}}}";
    }
}

public class ValidationBuilder
{
    private readonly Dictionary<string, string> _errors = new();

    public void Guard(string property, params (Func<bool> test, string error)[] tests)
    {
        if (_errors.ContainsKey(property))
            return;

        if (tests.Where(t => t.test()).Select(t => t.error).FirstOrDefault() is string error)
            _errors[property] = error;
    }

    public IValidationResult Build()
    {
        if (_errors.Count > 0)
            return new Invalid(_errors);

        return new Valid();
    }
}

public static class Validation
{
    // public static IInvalid? Guard(string property, params (Func<bool> test, string error)[] tests)
    // {
    //     if (tests.Where(t => t.test()).Select(t => t.error).FirstOrDefault() is string error)
    //         return new Invalid(new Dictionary<string, string>() { [property] = error });

    //     return null;
    // }

    // public static IInvalid<T>? Guard<T>(string property, params (Func<bool> test, string error)[] tests) where T : notnull
    // {
    //     if (tests.Where(t => t.test()).Select(t => t.error).FirstOrDefault() is string error)
    //         return new Invalid<T>(new Dictionary<string, string>() { [property] = error });

    //     return null;
    // }

    public static IValidationResult<T> Invalid<T>(string property, string error) where T : notnull
    {
        return new Invalid<T>(new Dictionary<string, string> { [property] = error });
    }

    public static IValidationResult<T> Invalid<T>(string error) where T : notnull
    {
        return new Invalid<T>(new Dictionary<string, string> { ["default"] = error });
    }

    public static IValidationResult Invalid(string property, string error)
    {
        return new Invalid(new Dictionary<string, string> { [property] = error });
    }

    public static IValidationResult Invalid(string error)
    {
        return new Invalid(new Dictionary<string, string> { ["default"] = error });
    }

    public static IValidationResult<T> Valid<T>(T value) where T : notnull
    {
        return new Valid<T>(value);
    }

    public static IValidationResult Valid()
    {
        return new Valid();
    }
}