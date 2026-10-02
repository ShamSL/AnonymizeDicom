using Microsoft.Extensions.Logging;

namespace AnonymizeDicom.Logging;

/// <summary>
/// Single shared logger factory for the whole app, so every service writes
/// to the same log file through one file handle.
/// </summary>
public static class Log
{
    private static readonly ILoggerFactory Factory =
        LoggerFactory.Create(builder =>
            builder.AddProvider(new SimpleFileLoggerProvider("anonymizeDicom.log")));

    public static ILogger<T> For<T>() => Factory.CreateLogger<T>();
}
