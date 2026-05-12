using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectPro.Models.Responses
{
    /// <summary>
    /// Represents a response from an operation containing a success indicator, message, and optional data.
    /// </summary>
    public class ObjectResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Gets or sets the associated data.
        /// </summary>
        public object Data { get; set; }
    }
}
