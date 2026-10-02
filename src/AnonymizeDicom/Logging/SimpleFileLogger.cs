using Microsoft.Extensions.Logging;

namespace AnonymizeDicom.Logging;

/// <summary>
/// Minimal file logging provider 
/// </summary>
public sealed class SimpleFileLoggerProvider : ILoggerProvider
{
    private readonly StreamWriter _writer;

    public SimpleFileLoggerProvider(string filePath)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _writer = new StreamWriter(filePath, append: true) { AutoFlush = true };
    }

    public ILogger CreateLogger(string categoryName) => new SimpleFileLogger(categoryName, _writer);

    public void Dispose() => _writer.Dispose();
}

public sealed class SimpleFileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly StreamWriter _writer;

    public SimpleFileLogger(string categoryName, StreamWriter writer)
    {
        _categoryName = categoryName;
        _writer = writer;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);

        // make it thread safe.
        lock (_writer)
        {
            _writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel,-11}] {_categoryName}: {message}");
            if (exception is not null)
            {
                _writer.WriteLine(exception);
            }
        }
    }
}
