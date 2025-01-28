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

        private StreamWriter logFileWriter;
        private string _logFilePath;
        private object lockObject = new object();
        private long maxLogFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public ErrorLogger(string logFilePath)
        {
            this._logFilePath = logFilePath;

            // Create the directory if it doesn't exist
            string logDirectory = System.IO.Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(logFilePath))
            {
                if (logDirectory != null)
                    Directory.CreateDirectory(logDirectory);
            }

            // Create or open the log file for appending
            logFileWriter = File.AppendText(this._logFilePath);
        }

        public async Task LogMessage(LogLevel level, string message, Exception ex = null)
        {
            try
            {
                if (ShouldLog(level))
                {
                    string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] - {message}";

                    if (ex != null)
                    {
                        logEntry += Environment.NewLine + ex.ToString();
                    }

                    await WriteToLogFileAsync(logEntry, cancellationTokenSource.Token);
                }
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Error while logging: {logEx.Message}");
            }
        }

        private bool ShouldLog(LogLevel level)
        {
            // Implement your log level filtering logic here.
            // Return true or false based on whether to log the message.
            return true;
        }

        private async Task WriteToLogFileAsync(string logEntry, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                lock (lockObject)
                {
                    // Check for cancellation
                    cancellationToken.ThrowIfCancellationRequested();

                    if (logFileWriter.BaseStream.Length + logEntry.Length >= maxLogFileSizeBytes)
                    {
                        // Perform log rotation logic here (e.g., create a new log file, archive the old one).
                    }

                    logFileWriter.WriteLine(logEntry);
                    logFileWriter.Flush();
                }
            }, cancellationToken);
        }

        public void Dispose()
        {
            lock (lockObject)
            {
                // Ensure that the log file is properly closed when the object is disposed
                logFileWriter?.Close();
                logFileWriter?.Dispose();
            }

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }

        public void ChangeLogFile(string newLogFilePath)
        {
            Dispose();
            _logFilePath = newLogFilePath;
            logFileWriter = File.AppendText(_logFilePath);
        }
    }
}
