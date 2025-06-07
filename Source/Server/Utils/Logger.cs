using System.Collections.Concurrent;
using System.Text;

namespace CelestialLeague.Server.Utils
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Critical = 4
    }

    public class Logger : IDisposable
    {
        private readonly ConcurrentQueue<LogEntry> _logQueue = new();
        private readonly Timer _flushTimer;
        private readonly object _fileLock = new();
        private readonly string _logDirectory;
        private readonly string _logFileName;
        private readonly LogLevel _minLogLevel;
        private readonly bool _enableConsoleOutput;
        private readonly bool _enableFileOutput;
        private bool _disposed;

        public Logger(LogLevel minLogLevel = LogLevel.Info, bool enableConsoleOutput = true, bool enableFileOutput = true)
        {
            _minLogLevel = minLogLevel;
            _enableConsoleOutput = enableConsoleOutput;
            _enableFileOutput = enableFileOutput;
            
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            _logFileName = $"server_{DateTime.Now:yyyy-MM-dd}.log";
            
            if (_enableFileOutput)
            {
                EnsureLogDirectoryExists();
            }
            
            _flushTimer = new Timer(FlushLogs, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public void Info(string message) => Log(LogLevel.Info, message);
        public void Error(string message, Exception? ex = null) => Log(LogLevel.Error, message, ex);
        public void Warning(string message) => Log(LogLevel.Warning, message);
        public void Debug(string message) => Log(LogLevel.Debug, message);

        private void Log(LogLevel level, string message, Exception? exception = null)
        {
            if (level < _minLogLevel || _disposed)
                return;

            _logQueue.Enqueue(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Message = message,
                Exception = exception
            });
        }

        private void FlushLogs(object? state)
        {
            if (_disposed) return;

            var entries = new List<LogEntry>();
            while (_logQueue.TryDequeue(out var entry))
            {
                entries.Add(entry);
            }

            foreach (var entry in entries)
            {
                var formatted = FormatLogEntry(entry);
                
                if (_enableConsoleOutput)
                    WriteToConsole(entry.Level, formatted);
                    
                if (_enableFileOutput)
                    WriteToFile(formatted);
            }
        }

        private static string FormatLogEntry(LogEntry entry)
        {
            var message = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] {entry.Message}";
            if (entry.Exception != null)
                message += $"\nException: {entry.Exception}";
            return message;
        }

        private static void WriteToConsole(LogLevel level, string message)
        {
            var color = level switch
            {
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Info => ConsoleColor.White,
                LogLevel.Debug => ConsoleColor.Gray,
                _ => ConsoleColor.White
            };

            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }

        private void WriteToFile(string message)
        {
#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                lock (_fileLock)
                {
                    var path = Path.Combine(_logDirectory, _logFileName);
                    File.AppendAllText(path, message + Environment.NewLine);
                }
            }
            catch
            {
                // silent fail to prevent logging from crashing server
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        private void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // dispose managed resources
                    _flushTimer?.Dispose();
                    FlushLogs(null); // final flush
                }
                
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public LogLevel Level { get; set; }
            public string Message { get; set; } = string.Empty;
            public Exception? Exception { get; set; }
        }
    }
}
