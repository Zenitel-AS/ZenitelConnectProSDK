using System;
using System.Collections.Generic;
using System.Text;
using Wamp.Client;

namespace ConnectPro.Models.Responses
{
    /// <summary>
    /// Represents the outcome of a call-related operation executed through the SDK.
    /// </summary>
    public class CallResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation succeeded.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the raw WAMP response type associated with the operation.
        /// </summary>
        public WampClient.ResponseType WampResponse { get; set; }

        /// <summary>
        /// Gets or sets a human-readable completion message for the operation.
        /// </summary>
        public string CompletionText { get; set; }

        /// <summary>
        /// Gets or sets the exception associated with the operation when it fails unexpectedly.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallResponse"/> class.
        /// </summary>
        public CallResponse()
        {
            Success = false;
            WampResponse = WampClient.ResponseType.WampNoResponce;
            CompletionText = string.Empty;
        }

        /// <summary>
        /// Creates a <see cref="CallResponse"/> from a raw WAMP response payload.
        /// </summary>
        /// <param name="response">The raw WAMP response returned by the backend.</param>
        /// <returns>A normalized call response describing the operation outcome.</returns>
        public static CallResponse FromWampResponse(WampClient.wamp_response response)
        {
            if (response == null)
            {
                return new CallResponse
                {
                    Success = false,
                    WampResponse = WampClient.ResponseType.WampNoResponce,
                    CompletionText = "No WAMP response returned."
                };
            }

            return new CallResponse
            {
                Success = response.WampResponse == WampClient.ResponseType.WampRequestSucceeded,
                WampResponse = response.WampResponse,
                CompletionText = response.CompletionText ?? string.Empty
            };
        }

        /// <summary>
        /// Creates a failed <see cref="CallResponse"/> from an exception.
        /// </summary>
        /// <param name="ex">The exception that caused the operation to fail.</param>
        /// <returns>A call response containing the exception details.</returns>
        public static CallResponse FromException(Exception ex)
        {
            return new CallResponse
            {
                Success = false,
                WampResponse = WampClient.ResponseType.WampRequestFailed,
                CompletionText = ex?.Message ?? "Unknown exception.",
                Exception = ex
            };
        }
    }
}
