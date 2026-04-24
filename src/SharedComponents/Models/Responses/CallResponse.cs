using System;
using System.Collections.Generic;
using System.Text;
using Wamp.Client;

namespace ConnectPro.Models.Responses
{
    public class CallResponse
    {
        public bool Success { get; set; }
        public WampClient.ResponseType WampResponse { get; set; }
        public string CompletionText { get; set; }
        public Exception Exception { get; set; }

        public CallResponse()
        {
            Success = false;
            WampResponse = WampClient.ResponseType.WampNoResponce;
            CompletionText = string.Empty;
        }

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
