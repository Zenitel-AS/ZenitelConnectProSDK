using System;
using System.Threading.Tasks;
using Wamp.Client;

namespace ConnectPro
{
    /// <summary>
    /// Monitors system events, handles exceptions, and logs WAMP client messages.
    /// </summary>
    public class SystemMonitor : IDisposable
    {
        #region Fields

        private readonly object _lock = new object();
        private readonly WampClient _wamp;
        private readonly Events _events;
        private readonly Configuration _configuration;
        private Tools.ErrorLogger _errorLogger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemMonitor"/> class.
        /// </summary>
        /// <param name="events">The event system used for exception handling.</param>
        /// <param name="configuration">The application configuration containing log file paths.</param>
        public SystemMonitor(Events events, Configuration configuration)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _events.OnExceptionThrown += HandleExceptionThrown;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemMonitor"/> class with WAMP client monitoring.
        /// </summary>
        /// <param name="events">The event system used for exception handling.</param>
        /// <param name="configuration">The application configuration containing log file paths.</param>
        /// <param name="_wamp">The WAMP client used for handling log messages.</param>
        public SystemMonitor(Events events, Configuration configuration, ref WampClient wamp)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _events.OnExceptionThrown += HandleExceptionThrown;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _wamp = wamp ?? throw new ArgumentNullException(nameof(wamp));

            if (_wamp != null)
            {
                _wamp.OnChildLogString += HandleWampChildLogString;
            }
        }
        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles exceptions thrown within the system and logs them.
        /// </summary>
        /// <param name="sender">The source of the exception.</param>
        /// <param name="ex">The exception that was thrown.</param>
        public void HandleExceptionThrown(object sender, Exception ex)
        {
            if (ex == null) return;

            lock (_lock)
            {
                using (_errorLogger = new Tools.ErrorLogger(_configuration.ErrorLogFilePath))
                {
                    Task.Run(async () => await _errorLogger.LogMessage(Tools.ErrorLogger.LogLevel.Error, ex.Message, ex));
                }
            }
        }

        /// <summary>
        /// Handles log messages from the WAMP client and logs them as warnings.
        /// </summary>
        /// <param name="sender">The source of the log message.</param>
        /// <param name="ex">The log message string.</param>
        private void HandleWampChildLogString(object sender, string ex)
        {
            if (string.IsNullOrEmpty(ex)) return;

            lock (_lock)
            {
                using (_errorLogger = new Tools.ErrorLogger(_configuration.ErrorLogFilePath))
                {
                    Task.Run(async () => await _errorLogger.LogMessage(Tools.ErrorLogger.LogLevel.Warning, ex));
                }
            }
        }

        #endregion

        #region IDisposable Implementation

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Unsubscribe from events
                if (_events != null)
                {
                    _events.OnExceptionThrown -= HandleExceptionThrown;
                }

                // If WAMP client subscription exists, unsubscribe
                if (_wamp != null)
                {
                    _wamp.OnChildLogString -= HandleWampChildLogString;
                }

                // Dispose managed resources explicitly if any
                if (_errorLogger != null)
                {
                    _errorLogger.Dispose();
                    _errorLogger = null;
                }
            }

            _disposed = true;
        }
        #endregion
    }
}
