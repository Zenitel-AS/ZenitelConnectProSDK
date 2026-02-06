#pragma warning disable CS1591
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Zenitel.IntegrationModule.REST
{
    /// <summary>
    /// This class implements a REST client for communicating with the Zenitel Connect server.
    /// Supports both OAuth2 authentication and fallback to Basic Authentication.
    /// </summary>
    public class RestClient
    {
        /// <summary>Zenitel Connect Server IP Address.</summary>
        public string ServerAddress = "169.254.1.5";

        /// <summary>Zenitel Connect Server Access User Name (or OAuth2 client_id)</summary>
        public string UserName = string.Empty;

        /// <summary>Zenitel Connect Server Access Password (or OAuth2 client_secret)</summary>
        public string Password = string.Empty;

        /// <summary>Use encrypted connection (HTTPS)</summary>
        public bool UseEncryption = true;

        /// <summary>This string defines the port number used for HTTPS communication</summary>
        public string HttpEncryptedPort = "443";

        /// <summary>This string defines the port number used for HTTP communication</summary>
        public string HttpUnencryptedPort = "80";

        /// <summary>Request timeout in milliseconds</summary>
        public int Timeout = 5000;

        /// <summary>OAuth2 access token for authenticated requests</summary>
        private string _accessToken = string.Empty;

        /// <summary>OAuth2 access token expiration time</summary>
        private DateTime _tokenExpirationTime = DateTime.MinValue;

        /// <summary>Lock for thread-safe token management</summary>
        private readonly object _tokenLock = new object();

        /// <summary>Event Handler for logging text.</summary>
        public event EventHandler<string> OnLogString;

        /// <summary>Event Handler for errors.</summary>
        public event EventHandler<string> OnError;

        /// <summary>
        /// Gets whether the client has a valid access token.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                lock (_tokenLock)
                {
                    return !string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpirationTime;
                }
            }
        }

        /// <summary>
        /// DTO for OAuth2 token response.
        /// </summary>
        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonProperty("token_type")]
            public string TokenType { get; set; }
        }

        /// <summary>
        /// Authenticates with the Zenitel server using OAuth2 password flow.
        /// </summary>
        /// <returns>True if authentication was successful, false otherwise</returns>
        public async Task<bool> AuthenticateAsync()
        {
            return await AuthenticateAsync(CancellationToken.None);
        }

        /// <summary>
        /// Authenticates with the Zenitel server using OAuth2 password flow with cancellation support.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if authentication was successful, false otherwise</returns>
        public async Task<bool> AuthenticateAsync(CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password))
                {
                    OnLogString?.Invoke(this, "RestClient.AuthenticateAsync() - UserName or Password not set");
                    return false;
                }

                ct.ThrowIfCancellationRequested();

                // Build OAuth2 login request
                var loginRequest = new
                {
                    client_id = UserName,
                    client_secret = Password,
                    grant_type = "password",
                    username = UserName,
                    password = Password
                };

                string jsonBody = JsonConvert.SerializeObject(loginRequest);

                // Send authentication request
                string response = await SendRequestAsync("POST", "/api/auth/login", jsonBody, ct, useOAuth2: false);

                if (string.IsNullOrEmpty(response))
                {
                    OnError?.Invoke(this, "RestClient.AuthenticateAsync() - Empty response from authentication endpoint");
                    return false;
                }

                // Parse token response
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(response);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    OnError?.Invoke(this, "RestClient.AuthenticateAsync() - Invalid token response");
                    return false;
                }

                // Store token and expiration time
                lock (_tokenLock)
                {
                    _accessToken = tokenResponse.AccessToken;
                    // Set expiration to 90% of token lifetime to refresh before actual expiration
                    _tokenExpirationTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn * 0.9);
                }

                OnLogString?.Invoke(this, $"RestClient.AuthenticateAsync() - Authentication successful. Token expires in {tokenResponse.ExpiresIn} seconds");
                return true;
            }
            catch (OperationCanceledException)
            {
                OnLogString?.Invoke(this, "RestClient.AuthenticateAsync() - Authentication cancelled");
                return false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, $"RestClient.AuthenticateAsync() - Authentication failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ensures the OAuth2 token is valid, refreshing if necessary.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if a valid token is available, false otherwise</returns>
        public async Task<bool> EnsureAuthenticatedAsync(CancellationToken ct)
        {
            lock (_tokenLock)
            {
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpirationTime)
                {
                    return true;
                }
            }

            // Token missing or expired, re-authenticate
            return await AuthenticateAsync(ct);
        }

        /// <summary>
        /// Clears the stored OAuth2 token, forcing re-authentication on next request.
        /// </summary>
        public void ClearToken()
        {
            lock (_tokenLock)
            {
                _accessToken = string.Empty;
                _tokenExpirationTime = DateTime.MinValue;
            }
            OnLogString?.Invoke(this, "RestClient.ClearToken() - OAuth2 token cleared");
        }

        /// <summary>
        /// Validates the remote certificate for HTTPS connections.
        /// </summary>
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Accept any certificate for development/testing purposes
            // In production, implement proper certificate validation
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            return true;
        }


        /// <summary>
        /// Performs a GET request to the specified API endpoint.
        /// </summary>
        /// <param name="endpoint">The API endpoint (e.g., "/api/system/devices_accounts")</param>
        /// <returns>The response content as a string</returns>
        public async Task<string> GetAsync(string endpoint)
        {
            return await SendRequestAsync("GET", endpoint, null, CancellationToken.None);
        }

        /// <summary>
        /// Performs a GET request to the specified API endpoint with cancellation support.
        /// </summary>
        /// <param name="endpoint">The API endpoint (e.g., "/api/system/devices_accounts")</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The response content as a string</returns>
        public async Task<string> GetAsync(string endpoint, CancellationToken ct)
        {
            return await SendRequestAsync("GET", endpoint, null, ct);
        }


        /// <summary>
        /// Performs a GET request to the specified API endpoint and deserializes the response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response into</typeparam>
        /// <param name="endpoint">The API endpoint (e.g., "/api/system/devices_accounts")</param>
        /// <returns>The deserialized response object</returns>
        public async Task<T> GetAsync<T>(string endpoint)
        {
            string response = await GetAsync(endpoint);
            return JsonConvert.DeserializeObject<T>(response);
        }

        /// <summary>
        /// Performs a GET request to the specified API endpoint and deserializes the response with cancellation support.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response into</typeparam>
        /// <param name="endpoint">The API endpoint (e.g., "/api/system/devices_accounts")</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The deserialized response object</returns>
        public async Task<T> GetAsync<T>(string endpoint, CancellationToken ct)
        {
            string response = await GetAsync(endpoint, ct);
            return JsonConvert.DeserializeObject<T>(response);
        }


        /// <summary>
        /// Performs a POST request to the specified API endpoint.
        /// </summary>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="body">The request body as a string</param>
        /// <returns>The response content as a string</returns>
        public async Task<string> PostAsync(string endpoint, string body)
        {
            return await SendRequestAsync("POST", endpoint, body, CancellationToken.None);
        }

        /// <summary>
        /// Performs a POST request to the specified API endpoint with cancellation support.
        /// </summary>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="body">The request body as a string</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The response content as a string</returns>
        public async Task<string> PostAsync(string endpoint, string body, CancellationToken ct)
        {
            return await SendRequestAsync("POST", endpoint, body, ct);
        }


        /// <summary>
        /// Performs a POST request with JSON body to the specified API endpoint and deserializes the response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response into</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="body">The request body object to serialize as JSON</param>
        /// <returns>The deserialized response object</returns>
        public async Task<T> PostAsync<T>(string endpoint, object body)
        {
            string jsonBody = JsonConvert.SerializeObject(body);
            string response = await PostAsync(endpoint, jsonBody);
            return JsonConvert.DeserializeObject<T>(response);
        }

        /// <summary>
        /// Performs a POST request with JSON body to the specified API endpoint and deserializes the response with cancellation support.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response into</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="body">The request body object to serialize as JSON</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The deserialized response object</returns>
        public async Task<T> PostAsync<T>(string endpoint, object body, CancellationToken ct)
        {
            string jsonBody = JsonConvert.SerializeObject(body);
            string response = await PostAsync(endpoint, jsonBody, ct);
            return JsonConvert.DeserializeObject<T>(response);
        }


        /// <summary>
        /// Performs a POST request with JSON body (object) to the specified API endpoint with cancellation support.
        /// </summary>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="body">The request body object to serialize as JSON</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The response content as a string</returns>
        public async Task<string> PostAsync(string endpoint, object body, CancellationToken ct)
        {
            string jsonBody = JsonConvert.SerializeObject(body);
            return await PostAsync(endpoint, jsonBody, ct);
        }


        /// <summary>
        /// Performs a DELETE request to the specified API endpoint.
        /// </summary>
        /// <param name="endpoint">The API endpoint</param>
        /// <returns>The response content as a string</returns>
        public async Task<string> DeleteAsync(string endpoint)
        {
            return await SendRequestAsync("DELETE", endpoint, null, CancellationToken.None);
        }

        /// <summary>
        /// Performs a DELETE request to the specified API endpoint with cancellation support.
        /// </summary>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The response content as a string</returns>
        public async Task<string> DeleteAsync(string endpoint, CancellationToken ct)
        {
            return await SendRequestAsync("DELETE", endpoint, null, ct);
        }


        /// <summary>
        /// Performs a PUT request to the specified API endpoint.
        /// </summary>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="body">The request body as a string</param>
        /// <returns>The response content as a string</returns>
        public async Task<string> PutAsync(string endpoint, string body)
        {
            return await SendRequestAsync("PUT", endpoint, body, CancellationToken.None);
        }

        /// <summary>
        /// Performs a PUT request to the specified API endpoint with cancellation support.
        /// </summary>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="body">The request body as a string</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The response content as a string</returns>
        public async Task<string> PutAsync(string endpoint, string body, CancellationToken ct)
        {
            return await SendRequestAsync("PUT", endpoint, body, ct);
        }


        /// <summary>
        /// Performs a PUT request with JSON body to the specified API endpoint and deserializes the response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response into</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="body">The request body object to serialize as JSON</param>
        /// <returns>The deserialized response object</returns>
        public async Task<T> PutAsync<T>(string endpoint, object body)
        {
            string jsonBody = JsonConvert.SerializeObject(body);
            string response = await PutAsync(endpoint, jsonBody);
            return JsonConvert.DeserializeObject<T>(response);
        }

        /// <summary>
        /// Performs a PUT request with JSON body to the specified API endpoint and deserializes the response with cancellation support.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response into</typeparam>
        /// <param name="endpoint">The API endpoint</param>
        /// <param name="body">The request body object to serialize as JSON</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>The deserialized response object</returns>
        public async Task<T> PutAsync<T>(string endpoint, object body, CancellationToken ct)
        {
            string jsonBody = JsonConvert.SerializeObject(body);
            string response = await PutAsync(endpoint, jsonBody, ct);
            return JsonConvert.DeserializeObject<T>(response);
        }


        /// <summary>
        /// Sends an HTTP request to the server using the configured credentials and OAuth2 authentication.
        /// </summary>
        private async Task<string> SendRequestAsync(string method, string endpoint, string body, CancellationToken ct, bool useOAuth2 = true)
        {
            try
            {
                // Check if cancellation was requested before starting
                ct.ThrowIfCancellationRequested();

                OnLogString?.Invoke(this, $"RestClient.SendRequestAsync() - Method: {method}, Endpoint: {endpoint}");

                // For non-auth endpoints, use basic auth; for regular endpoints, use OAuth2
                if (useOAuth2 && endpoint != "/api/auth/login")
                {
                    // Ensure we have a valid OAuth2 token
                    if (!await EnsureAuthenticatedAsync(ct))
                    {
                        throw new UnauthorizedAccessException("Failed to obtain OAuth2 token. Authentication required.");
                    }
                }

                // Build the URI
                string protocol = UseEncryption ? "https" : "http";
                string port = UseEncryption ? HttpEncryptedPort : HttpUnencryptedPort;
                string uri_str = $"{protocol}://{ServerAddress}:{port}{endpoint}";
                Uri uri = new Uri(uri_str);

                // Create the HTTP request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = method;
                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.Timeout = Timeout;

                // Add Authorization header
                if (useOAuth2 && endpoint != "/api/auth/login")
                {
                    // Use OAuth2 Bearer token for regular API calls
                    lock (_tokenLock)
                    {
                        if (!string.IsNullOrEmpty(_accessToken))
                        {
                            request.Headers.Add("Authorization", "Bearer " + _accessToken);
                        }
                    }
                }
                else
                {
                    // Use Basic Auth for authentication endpoint
                    string encoded = Convert.ToBase64String(
                        Encoding.GetEncoding("ISO-8859-1").GetBytes(UserName + ":" + Password));
                    request.Headers.Add("Authorization", "Basic " + encoded);
                }

                // Configure SSL/TLS
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
#if NET48
                // .NET Framework 4.8: Use TLS 1.3, 1.2, 1.1, and 1.0
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls13 |
                    SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls;
#else
                // Other frameworks: Use TLS 1.2, 1.1, and 1.0
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls12 |
                    SecurityProtocolType.Tls11 |
                    SecurityProtocolType.Tls;
#endif

                // Write the request body if provided
                if (!string.IsNullOrEmpty(body))
                {
                    byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
                    request.ContentLength = bodyBytes.Length;
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(bodyBytes, 0, bodyBytes.Length);
                    }
                }

                // Create a task that will be cancelled if the cancellation token is cancelled
                using (ct.Register(() => request.Abort()))
                {
                    // Send the request and get the response
                    HttpWebResponse response = (HttpWebResponse)await Task.Factory.FromAsync(
                        (callback, state) => request.BeginGetResponse(callback, state),
                        request.EndGetResponse,
                        null).ConfigureAwait(false);

                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            string responseContent = await reader.ReadToEndAsync().ConfigureAwait(false);
                            OnLogString?.Invoke(this, $"RestClient.SendRequestAsync() - Response received successfully");
                            return responseContent;
                        }
                    }
                    else
                    {
                        string errorMessage = $"HTTP request error: {response.StatusCode} {response.StatusDescription}";
                        OnError?.Invoke(this, errorMessage);
                        throw new Exception(errorMessage);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                OnLogString?.Invoke(this, "RestClient.SendRequestAsync() - Request cancelled");
                throw;
            }
            catch (Exception ex)
            {
                string errorMessage = $"RestClient.SendRequestAsync() Exception: {ex.Message}";
                OnLogString?.Invoke(this, errorMessage);
                OnError?.Invoke(this, errorMessage);
                throw;
            }
        }
    }
}
