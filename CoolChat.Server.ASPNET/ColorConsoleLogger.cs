using Microsoft.Extensions.Logging;

namespace CoolChat.Server.ASPNET;

public sealed class ColorConsoleLoggerConfiguration
{
    public int EventId { get; set; }
    public Dictionary<LogLevel, ConsoleColor> ColorMap { get; set; } = new()
    {
        [LogLevel.Critical] = ConsoleColor.DarkRed,
        [LogLevel.Debug] = ConsoleColor.Green,
        [LogLevel.Error] = ConsoleColor.Red,
        [LogLevel.Information] = ConsoleColor.Cyan,
        [LogLevel.Trace] = ConsoleColor.Gray,
        [LogLevel.Warning] = ConsoleColor.Yellow,
    };
    public HashSet<LogLevel> Mask { get; set; } = new()
    {
        LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical
    };
}

public class ColorConsoleLogger : ILogger
{
    private readonly string _name;
    private readonly Func<ColorConsoleLoggerConfiguration> _getConfig;
    private const int LogLevelPadding = 11; // Longest LogLevel string (Information) = 11

    public ColorConsoleLogger(string name, Func<ColorConsoleLoggerConfiguration> getConfig) =>
        (_name, _getConfig) = (name, getConfig);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public bool IsEnabled(LogLevel logLevel) =>
        _getConfig().Mask.Contains(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;
        
        ColorConsoleLoggerConfiguration config = _getConfig();

        if (config.EventId != 0 && config.EventId != eventId)
            return;
        
        ConsoleColor originalColor = Console.ForegroundColor;

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write('[');

        Console.ForegroundColor = config.ColorMap[logLevel];
        Console.Write(logLevel.ToString().PadLeft(LogLevelPadding));

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("]: ");


        IEnumerable<string> lines = formatter(state, exception).Split("\n")
                                                               .SelectMany(s => s.Chunk(Console.BufferWidth - 15)
                                                                                 .Select(s => new string(s)));
        Console.ForegroundColor = config.ColorMap[logLevel];
        Console.Write(lines.First());

        foreach (string line in lines.Skip(1))
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n            |  ");
            Console.ForegroundColor = config.ColorMap[logLevel];
            Console.Write(line);
        }

        Console.ForegroundColor = originalColor;
        Console.WriteLine();
    }
}