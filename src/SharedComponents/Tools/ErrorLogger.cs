using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectPro.Tools
{
    /// <summary>
    /// Provides logging functionality for errors, warnings, and informational messages.
    /// Supports log file rotation and ensures thread-safe writing.
    /// Implements <see cref="IDisposable"/> for resource management.
    /// </summary>
    public class ErrorLogger : IDisposable
    {
        #region Enums

        /// <summary>
        /// Defines the severity levels for logging messages.
        /// </summary>
        public enum LogLevel
        {
            /// <summary>Informational messages.</summary>
            Info,

            /// <summary>Warning messages indicating potential issues.</summary>
            Warning,

            /// <summary>Error messages indicating failures.</summary>
            Error
        }

        #endregion

        #region Fields

        private bool disposed = false;
        private Lazy<StreamWriter> logFileWriterLazy;
        private string _logFilePath;
        private readonly object lockObject = new object();
        private readonly long maxLogFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLogger"/> class.
        /// </summary>
        /// <param name="logFilePath">The file path where logs should be stored.</param>
        public ErrorLogger(string logFilePath)
        {
            _logFilePath = logFilePath;

            // Ensure log directory exists
            string logDirectory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Create or open the log file for appending
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
                            using (FileStream fileStream = new FileStream(
                                _logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
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
                            Console.WriteLine($"Error while opening the log file: {ex.Message}");
                            throw;
                        }

                        Thread.Sleep(100);
                    }
                }
            });
        }

        #endregion

        #region Logging Methods

        /// <summary>
        /// Asynchronously logs a message with the specified log level.
        /// </summary>
        /// <param name="level">The severity level of the log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="ex">Optional exception details to include in the log.</param>
        public async Task LogMessage(LogLevel level, string message, Exception ex = null)
        {
            try
            {
                if (ShouldLog(message))
                {
                    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] - {message}\n{ex}";
                    await WriteToLogFileAsync(logEntry, cancellationTokenSource.Token);
                }
            }
            catch (Exception logEx)
            {
                HandleLoggingException(logEx);
            }
        }

        /// <summary>
        /// Determines whether a log message should be recorded based on log file size constraints.
        /// </summary>
        /// <param name="logEntry">The log message content.</param>
        /// <returns><c>true</c> if the message should be logged; otherwise, <c>false</c>.</returns>
        private bool ShouldLog(string logEntry)
        {
            lock (lockObject)
            {
                return logFileWriterLazy.Value.BaseStream.Length + logEntry.Length < maxLogFileSizeBytes;
            }
        }

        /// <summary>
        /// Asynchronously writes a log entry to the log file.
        /// </summary>
        /// <param name="logEntry">The log message to write.</param>
        /// <param name="cancellationToken">A cancellation token to handle task cancellation.</param>
        private async Task WriteToLogFileAsync(string logEntry, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                lock (lockObject)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!ShouldLog(logEntry))
                        {
                            RotateLogFile();
                        }

                        logFileWriterLazy.Value.WriteLine(logEntry);
                        logFileWriterLazy.Value.Flush();
                    }
                    catch (Exception ex)
                    {
                        HandleLoggingException(ex);
                    }
                }
            }, cancellationToken);
        }

        #endregion

        #region Log File Management

        /// <summary>
        /// Rotates the log file when it exceeds the maximum allowed size.
        /// The current log file is archived with a timestamped name.
        /// </summary>
        private void RotateLogFile()
        {
            string archiveFilePath = $"{_logFilePath}_{DateTime.Now:yyyyMMdd_HHmmss}.log";

            logFileWriterLazy.Value.Close();
            logFileWriterLazy.Value.Dispose();

            File.Move(_logFilePath, archiveFilePath);

            logFileWriterLazy = new Lazy<StreamWriter>(() =>
            {
                lock (lockObject)
                {
                    return File.AppendText(_logFilePath);
                }
            });
        }

        /// <summary>
        /// Handles exceptions that occur during logging operations.
        /// </summary>
        /// <param name="ex">The exception that occurred.</param>
        private void HandleLoggingException(Exception ex)
        {
            Console.WriteLine($"Error while logging: {ex.Message}");
        }

        /// <summary>
        /// Changes the log file path and resets the log writer.
        /// </summary>
        /// <param name="newLogFilePath">The new file path for logging.</param>
        public void ChangeLogFile(string newLogFilePath)
        {
            Dispose();
            _logFilePath = newLogFilePath;

            logFileWriterLazy = new Lazy<StreamWriter>(() =>
            {
                lock (lockObject)
                {
                    return File.AppendText(_logFilePath);
                }
            });
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Releases resources used by the <see cref="ErrorLogger"/> class.
        /// </summary>
        public void Dispose()
        {
            lock (lockObject)
            {
                if (!disposed && logFileWriterLazy.IsValueCreated)
                {
                    try
                    {
                        logFileWriterLazy.Value.Dispose();
                        disposed = true;
                    }
                    catch
                    {
                        // Suppress exceptions during disposal
                    }
                }
            }

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        #endregion
    }
}
