namespace WampSharp.Logging
{
    using System;

    /// <summary>
    ///     Simple interface that represent a logger.
    /// </summary>
    public interface ILog
    {
        /// <summary>
        ///     Log a message the specified log level.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="exception">An optional exception.</param>
        /// <param name="formatParameters">Optional format parameters for the message generated by the messagefunc. </param>
        /// <returns>true if the message was logged. Otherwise false.</returns>
        /// <remarks>
        ///     Note to implementers: the message func should not be called if the loglevel is not enabled
        ///     so as not to incur performance penalties.
        ///     To check IsEnabled call Log with only LogLevel and check the return value, no event will be written.
        /// </remarks>
        bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null,
            params object[] formatParameters);
    }
}
