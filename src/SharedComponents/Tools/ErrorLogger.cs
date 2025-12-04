using System;
using System.IO;
using System.Text;
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
                            FileStream fileStream = new FileStream(
                                _logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                            return new StreamWriter(fileStream);
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
        public Task LogMessage(LogLevel level, string message, Exception ex = null)
        {
            if (disposed)
            {
                // Logger is disposed; don't attempt to log.
                return Task.CompletedTask;
            }

            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] - {message}\n{ex}";
                return WriteToLogFileAsync(logEntry);
            }
            catch (Exception logEx)
            {
                HandleLoggingException(logEx);
                return Task.CompletedTask;
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
                try
                {
                    if (!logFileWriterLazy.IsValueCreated)
                    {
                        try
                        {
                            var _ = logFileWriterLazy.Value;
                        }
                        catch (Exception ex)
                        {
                            HandleLoggingException(ex);
                            return false;
                        }
                    }

                    if (logFileWriterLazy.Value?.BaseStream == null)
                        return false;

                    var encoding = logFileWriterLazy.Value.Encoding ?? Encoding.UTF8;
                    return logFileWriterLazy.Value.BaseStream.Length + encoding.GetByteCount(logEntry) < maxLogFileSizeBytes;
                }
                catch (Exception ex)
                {
                    HandleLoggingException(ex);
                    return false;
                }
            }
        }

        /// <summary>
        /// Asynchronously writes a log entry to the log file.
        /// </summary>
        /// <param name="logEntry">The log message to write.</param>
        private Task WriteToLogFileAsync(string logEntry)
        {
            // Keep async boundary but offload the actual file I/O to a background thread.
            return Task.Run(() =>
            {
                lock (lockObject)
                {
                    if (disposed)
                    {
                        return;
                    }

                    try
                    {
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
            });
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

            // Close current writer
            if (logFileWriterLazy.IsValueCreated)
            {
                logFileWriterLazy.Value.Close();
                logFileWriterLazy.Value.Dispose();
            }

            // Move current log file to archive
            if (File.Exists(_logFilePath))
            {
                File.Move(_logFilePath, archiveFilePath);
            }

            // Recreate writer for the original path
            logFileWriterLazy = new Lazy<StreamWriter>(() =>
            {
                lock (lockObject)
                {
                    FileStream fileStream = new FileStream(
                        _logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    return new StreamWriter(fileStream);
                }
            });
        }

        /// <summary>
        /// Handles exceptions that occur during logging operations.
        /// </summary>
        /// <param name="ex">The exception that occurred.</param>
        private void HandleLoggingException(Exception ex)
        {
            try
            {
                Console.WriteLine($"Error while logging: {ex.Message}");
            }
            catch
            {
                // Last-resort: never throw from logger error handling
            }
        }

        /// <summary>
        /// Changes the log file path and resets the log writer.
        /// </summary>
        /// <param name="newLogFilePath">The new file path for logging.</param>
        public void ChangeLogFile(string newLogFilePath)
        {
            lock (lockObject)
            {
                _logFilePath = newLogFilePath;

                // Dispose current writer, if any
                if (logFileWriterLazy.IsValueCreated)
                {
                    try
                    {
                        logFileWriterLazy.Value.Dispose();
                    }
                    catch
                    {
                        // Ignore dispose errors
                    }
                }

                // Recreate lazy writer with the new path
                logFileWriterLazy = new Lazy<StreamWriter>(() =>
                {
                    lock (lockObject)
                    {
                        FileStream fileStream = new FileStream(
                            _logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        return new StreamWriter(fileStream);
                    }
                });
            }
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
                if (disposed)
                    return;

                if (logFileWriterLazy.IsValueCreated)
                {
                    try
                    {
                        logFileWriterLazy.Value.Dispose();
                    }
                    catch
                    {
                        // Suppress exceptions during disposal
                    }
                }

                disposed = true;
            }
        }

        #endregion
    }
}
