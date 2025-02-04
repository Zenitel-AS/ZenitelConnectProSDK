using System;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents an exception log entry, storing details about an exception event.
    /// </summary>
    public class ExceptionLog
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the exception log entry.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of exception that occurred.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name or identifier of the sender that triggered the exception.
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the error message associated with the exception.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the exception occurred.
        /// </summary>
        public DateTime DateTime { get; set; }

        #endregion
    }
}
