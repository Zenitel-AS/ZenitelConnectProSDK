using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectPro.Tools
{
    public class ErrorLogger : IDisposable
    {
        public enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        private bool disposed = false;
        private Lazy<StreamWriter> logFileWriterLazy;
        private string _logFilePath;
        private object lockObject = new object();
        private long maxLogFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public ErrorLogger(string logFilePath)
        {
            this._logFilePath = logFilePath;

            // Create the directory if it doesn't exist (enhanced and optimized)
            string logDirectory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Create or open the log file for appending (thread-safe)
            logFileWriterLazy = new Lazy<StreamWriter>(() =>
            {
                const int maxRetries = 3;
                int retries = 0;

                while (true)
                {
                    try
                    {
                        lock (lockObject)
                        {
                            // Use FileStream to specify file-sharing options
                            using (FileStream fileStream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                            {
                                return new StreamWriter(fileStream);
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        retries++;

                        if (retries > maxRetries)
                        {
                            // Log the error or throw an exception if the maximum number of retries is reached
                            Console.WriteLine($"Error while opening the log file: {ex.Message}");
                            throw;
                        }

                        // Wait for a short duration before retrying
                        Thread.Sleep(100);
                    }
                }
            });

        }

        public async Task LogMessage(LogLevel level, string message, Exception ex = null)
        {
            try
            {
                if (ShouldLog(level, message))
                {
                    // Simplify log entry generation
                    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] - {message}\n{ex}";
                    await WriteToLogFileAsync(logEntry, cancellationTokenSource.Token);
                }
            }
            catch (Exception logEx)
            {
                // Consolidate exception handling
                HandleLoggingException(logEx);
            }
        }

        private bool ShouldLog(LogLevel level, string logEntry)
        {
            // Check the log file size within the lock to avoid race conditions
            lock (lockObject)
            {
                return logFileWriterLazy.Value.BaseStream.Length + logEntry.Length < maxLogFileSizeBytes;
            }
        }

        private async Task WriteToLogFileAsync(string logEntry, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                lock (lockObject)
                {
                    try
                    {
                        // Check for cancellation
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!ShouldLog(LogLevel.Info, logEntry))
                        {
                            RotateLogFile();
                        }

                        logFileWriterLazy.Value.WriteLine(logEntry);
                        logFileWriterLazy.Value.Flush();
                    }
                    catch (Exception ex)
                    {
                        // Consolidate exception handling
                        HandleLoggingException(ex);
                    }
                }
            }, cancellationToken);
        }

        private void RotateLogFile()
        {
            // Create a date-stamped archive of the current log file
            string archiveFilePath = $"{_logFilePath}_{DateTime.Now:yyyyMMdd_HHmmss}.log";

            // Close and dispose the current log file writer
            logFileWriterLazy.Value.Close();
            logFileWriterLazy.Value.Dispose();

            // Move the existing log file to the archive file
            File.Move(_logFilePath, archiveFilePath);

            // Create or open a new log file for appending
            logFileWriterLazy = new Lazy<StreamWriter>(() =>
            {
                lock (lockObject)
                {
                    return File.AppendText(this._logFilePath);
                }
            });
        }

        private void HandleLoggingException(Exception ex)
        {
            // Consolidated exception handling logic
            Console.WriteLine($"Error while logging: {ex.Message}");
        }

        public void Dispose()
        {
            lock (lockObject)
            {
                if (!disposed && logFileWriterLazy.IsValueCreated)
                {
                    try
                    {
                        logFileWriterLazy.Value.Dispose();
                        disposed = true; // Mark the object as disposed
                    }
                    catch
                    {

                    }
                }
            }

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        public void ChangeLogFile(string newLogFilePath)
        {
            Dispose();
            _logFilePath = newLogFilePath;
            logFileWriterLazy = new Lazy<StreamWriter>(() =>
            {
                lock (lockObject)
                {
                    return File.AppendText(this._logFilePath);
                }
            });
        }
    }
}
