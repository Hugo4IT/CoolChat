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
        [LogLevel.Warning] = ConsoleColor.Yellow
    };

    public HashSet<LogLevel> Mask { get; set; } = new()
    {
        LogLevel.Information, LogLevel.Warning, LogLevel.Error, LogLevel.Critical
    };
}

public class ColorConsoleLogger : ILogger
{
    private const int LogLevelPadding = 11; // Longest LogLevel string (Information) = 11

    // Use Mutex to prevent multiple threads writing to console at the same time
    // causing the log to scramble.
    private static readonly Mutex Lock = new();
    private readonly Func<ColorConsoleLoggerConfiguration> _getConfig;
    private readonly string _name;

    public ColorConsoleLogger(string name, Func<ColorConsoleLoggerConfiguration> getConfig)
    {
        (_name, _getConfig) = (name, getConfig);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return default!;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _getConfig().Mask.Contains(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var config = _getConfig();

        if (config.EventId != 0 && config.EventId != eventId)
            return;

        // Constructing message before lock to minimize blocked time
        var timestamp = $"{DateTime.Now:o} ";
        var padding = new string(' ', LogLevelPadding - logLevel.ToString().Length);
        var lines = formatter(state, exception).Split("\n")
            .SelectMany(s => s.Chunk(Console.BufferWidth - 49)
                .Select(s => new string(s)));
        var lineBreakPrefix = $"\n{new string(' ', 49)}";

        lock (Lock)
        {
            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(timestamp + padding);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write('[');

            Console.ForegroundColor = config.ColorMap[logLevel];
            Console.Write(logLevel);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("]: ");

            Console.ForegroundColor = config.ColorMap[logLevel];
            Console.Write(string.Join(lineBreakPrefix, lines));

            Console.ForegroundColor = originalColor;
            Console.WriteLine();
        }
    }
}