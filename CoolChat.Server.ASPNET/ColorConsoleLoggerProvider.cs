using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.Options;

namespace CoolChat.Server.ASPNET;

[UnsupportedOSPlatform("browser")]
[ProviderAlias("ColorConsole")]
public class ColorConsoleLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, ColorConsoleLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
    private readonly IDisposable? _onChangeToken;
    private ColorConsoleLoggerConfiguration _config;

    public ColorConsoleLoggerProvider(IOptionsMonitor<ColorConsoleLoggerConfiguration> config)
    {
        _config = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _config = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new ColorConsoleLogger(name, GetConfig));
    }

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }

    private ColorConsoleLoggerConfiguration GetConfig()
    {
        return _config;
    }
}