using ConnectPro;
using ConnectPro.Enums;
using ConnectPro.Models;
using Newtonsoft.Json;
using SharedComponents.Models.GPIO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zenitel.IntegrationModule.REST;

/// <summary>
/// REST-based GPIO transport implementation using the Zenitel Connect Pro REST API.
/// Provides GPIO point snapshots via REST endpoints instead of WAMP.
/// </summary>
public sealed class RestGpioTransport : IGpioTransport
{
    private readonly Core _core;

    /// <summary>
    /// Callback per device dirno.
    /// Note: REST polling does not provide real-time change events like WAMP does.
    /// This transport primarily supports snapshot retrieval.
    /// </summary>
    private readonly ConcurrentDictionary<string, Action<GpioPoint>> _callbacks =
        new ConcurrentDictionary<string, Action<GpioPoint>>();

    /// <summary>
    /// DTO for GPIO point response from REST API
    /// </summary>
    private class GpioResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }  // NEW: relay, gpi, gpo
    }

    /// <summary>
    /// DTO for GPIO list response from REST API
    /// </summary>
    private class GpioListResponse
    {
        [JsonProperty("gpos")]
        public List<GpioResponse> Gpos { get; set; }

        [JsonProperty("gpis")]
        public List<GpioResponse> Gpis { get; set; }
    }

    public RestGpioTransport(Core core)
    {
        _core = core ?? throw new ArgumentNullException(nameof(core));
    }

    /// <summary>
    /// Returns a snapshot of GPIO points (both GPIs and GPOs) by calling REST API endpoints.
    /// </summary>
    public async Task<IReadOnlyList<GpioPoint>> GetSnapshotAsync(string dirno, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(dirno))
            throw new ArgumentException("dirno must be provided.", nameof(dirno));

        if (ct.IsCancellationRequested)
            return await Task.FromCanceled<IReadOnlyList<GpioPoint>>(ct).ConfigureAwait(false);

        try
        {
            var list = new List<GpioPoint>();

            // DEBUG: Check authentication status
            System.Diagnostics.Debug.WriteLine($"RestGpioTransport.GetSnapshotAsync() - Dirno: {dirno}");
            System.Diagnostics.Debug.WriteLine($"RestGpioTransport.GetSnapshotAsync() - REST Authenticated: {_core.Rest.IsAuthenticated}");
            System.Diagnostics.Debug.WriteLine($"RestGpioTransport.GetSnapshotAsync() - Server: {_core.Rest.ServerAddress}");

            // Fetch GPOs
            try
            {
                // Try different endpoint formats - device;{dirno} with dirno query parameter
                List<GpioResponse> gposResponse = null;
                Exception lastException = null;

                var endpoint = $"/api/devices/device;dirno={dirno}/gpos";                          // Format 4: Simple gpos with query param

                try
                {
                    System.Diagnostics.Debug.WriteLine($"RestGpioTransport - Trying GPO endpoint: {endpoint}");

                    // Get raw response first to inspect structure
                    string rawResponse = await _core.Rest.GetAsync(endpoint, ct).ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine($"RestGpioTransport - GPO Response: {rawResponse?.Substring(0, Math.Min(100, rawResponse?.Length ?? 0))}...");

                    // Try to deserialize as nested array: List<List<GpioResponse>>
                    var nestedResponse = JsonConvert.DeserializeObject<List<List<GpioResponse>>>(rawResponse);

                    if (nestedResponse != null && nestedResponse.Count > 0)
                    {
                        // Flatten the nested array
                        gposResponse = new List<GpioResponse>();
                        foreach (var innerList in nestedResponse)
                        {
                            if (innerList != null)
                            {
                                gposResponse.AddRange(innerList);
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"✓ GPO endpoint succeeded: {endpoint}");
                        System.Diagnostics.Debug.WriteLine($"RestGpioTransport - GPOs retrieved: {gposResponse.Count}");
                        MapSnapshot(list, gposResponse, GpioDirection.Gpo);
                    }
                }
                catch (System.Net.WebException ex) when (ex.Response is System.Net.HttpWebResponse response && (int)response.StatusCode == 404)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Not Found: {endpoint}");
                    lastException = ex;
                }
                catch (System.Net.WebException ex) when (ex.Response is System.Net.HttpWebResponse response && (int)response.StatusCode == 403)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Forbidden: {endpoint}");
                    lastException = ex;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Error on {endpoint}: {ex.Message}");
                    lastException = ex;
                }


                if (gposResponse == null && lastException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to retrieve GPOs - tried all formats");
                    System.Diagnostics.Debug.WriteLine($"Last error: {lastException.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GPO retrieval: {ex.Message}");
            }

            // Fetch GPIs
            try
            {
                List<GpioResponse> gpisResponse = null;
                Exception lastException = null;

                var endpoint = $"/api/devices/device;dirno={dirno}/gpis";

                try
                {
                    System.Diagnostics.Debug.WriteLine($"RestGpioTransport - Trying GPI endpoint: {endpoint}");

                    // Get raw response first
                    string rawResponse = await _core.Rest.GetAsync(endpoint, ct).ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine($"RestGpioTransport - GPI Response: {rawResponse?.Substring(0, Math.Min(100, rawResponse?.Length ?? 0))}...");

                    // Try to deserialize as nested array: List<List<GpioResponse>>
                    var nestedResponse = JsonConvert.DeserializeObject<List<List<GpioResponse>>>(rawResponse);

                    if (nestedResponse != null && nestedResponse.Count > 0)
                    {
                        // Flatten the nested array
                        gpisResponse = new List<GpioResponse>();
                        foreach (var innerList in nestedResponse)
                        {
                            if (innerList != null)
                            {
                                gpisResponse.AddRange(innerList);
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"✓ GPI endpoint succeeded: {endpoint}");
                        System.Diagnostics.Debug.WriteLine($"RestGpioTransport - GPIs retrieved: {gpisResponse.Count}");
                        MapSnapshot(list, gpisResponse, GpioDirection.Gpi);
                    }
                }
                catch (System.Net.WebException ex) when (ex.Response is System.Net.HttpWebResponse response && (int)response.StatusCode == 404)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Not Found: {endpoint}");
                    lastException = ex;
                }
                catch (System.Net.WebException ex) when (ex.Response is System.Net.HttpWebResponse response && (int)response.StatusCode == 403)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Forbidden: {endpoint}");
                    lastException = ex;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"✗ Error on {endpoint}: {ex.Message}");
                    lastException = ex;
                }


                if (gpisResponse == null && lastException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to retrieve GPIs - tried all formats");
                    System.Diagnostics.Debug.WriteLine($"Last error: {lastException.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GPI retrieval: {ex.Message}");
            }

            return list;
        }
        catch (OperationCanceledException)
        {
            return await Task.FromCanceled<IReadOnlyList<GpioPoint>>(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting GPIO snapshot for {dirno}: {ex.Message}");
            return new List<GpioPoint>();
        }
    }

    /// <summary>
    /// Sets a GPO output state via REST API.
    /// </summary>
    public async Task SetGpoAsync(string dirno, string gpoId, bool active, int? timeSeconds, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(dirno))
            throw new ArgumentException("dirno must be provided.", nameof(dirno));

        if (ct.IsCancellationRequested)
            return;

        try
        {
            // Use gpoId directly (e.g., "relay1", "gpio2")
            string id = gpoId;

            // Build the request body
            var requestBody = new
            {
                id = id,
                operation = active ? "set" : "clear",
                time = timeSeconds ?? 0
            };

            // Try different endpoint formats - device;{dirno} with dirno query parameter
            var endpoint = $"/api/devices/device;dirno={dirno}/gpos";                           // Format 4: Simple gpos with dirno query param


            Exception lastException = null;

            try
            {
                System.Diagnostics.Debug.WriteLine($"RestGpioTransport.SetGpoAsync() - Trying endpoint: {endpoint}");

                await _core.Rest.PostAsync(endpoint, requestBody, ct).ConfigureAwait(false);

                System.Diagnostics.Debug.WriteLine($"✓ SetGPO succeeded on: {endpoint}");
                return;  // Success
            }
            catch (System.Net.WebException ex) when (ex.Response is System.Net.HttpWebResponse response && (int)response.StatusCode == 404)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Not Found: {endpoint}");
                lastException = ex;
            }
            catch (System.Net.WebException ex) when (ex.Response is System.Net.HttpWebResponse response && (int)response.StatusCode == 403)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Forbidden: {endpoint}");
                lastException = ex;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ Error on {endpoint}: {ex.Message}");
                lastException = ex;
            }


            if (lastException != null)
            {
                System.Diagnostics.Debug.WriteLine($"RestGpioTransport.SetGpoAsync() - Failed on all endpoint formats");
                System.Diagnostics.Debug.WriteLine($"Last error: {lastException.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            // Cancelled by caller - safe to return
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting GPO for {dirno}: {ex.Message}");
            // Don't throw - allow graceful degradation
        }
    }

    /// <summary>
    /// Registers a callback for GPIO point changes.
    /// Note: REST transport primarily supports snapshot retrieval.
    /// Real-time change events would require polling or WebSocket implementation.
    /// </summary>
    public void EnsureSubscribed(string dirno, Action<GpioPoint> onPoint)
    {
        if (string.IsNullOrEmpty(dirno))
            throw new ArgumentException("dirno must be provided.", nameof(dirno));
        if (onPoint == null)
            throw new ArgumentNullException(nameof(onPoint));

        // Register callback
        _callbacks[dirno] = onPoint;

        // Note: For real-time changes, implement polling or WebSocket integration later
    }

    /// <summary>
    /// Removes the callback for a specific device.
    /// </summary>
    public void DisposeFor(string dirno)
    {
        if (string.IsNullOrEmpty(dirno))
            return;

        _callbacks.TryRemove(dirno, out _);
    }

    // ============= Parsing helpers =============

    /// <summary>
    /// Parses the GPIO state from the state string.
    /// Recognizes: "high"/"low", "1"/"0", "active"/"inactive"
    /// </summary>
    private static GpioState ParseState(string stateStr)
    {
        if (string.IsNullOrEmpty(stateStr))
            return GpioState.Inactive;

        var normalized = stateStr.ToLowerInvariant();

        // Check for "high", "active", "1"
        if (normalized == "high" || normalized == "active" || normalized == "1")
            return GpioState.Active;

        // Default to inactive for "low", "inactive", "0", or unknown
        return GpioState.Inactive;
    }

    /// <summary>
    /// Maps the REST API response to a list of GpioPoint objects.
    /// </summary>
    private static void MapSnapshot(List<GpioPoint> list, IEnumerable<GpioResponse> elements, GpioDirection direction)
    {
        if (elements == null)
            return;

        foreach (var element in elements)
        {
            if (element == null)
                continue;

            var point = new GpioPoint(
                element.Id,
                direction,
                ParseState(element.State),
                DateTimeOffset.UtcNow,
                $"{{id: {element.Id}, state: {element.State}}}");

            list.Add(point);
        }
    }

    // ============= Advanced: Optional POST endpoint handling =============

    /// <summary>
    /// Overload: POST with generic response type (for future extensibility)
    /// </summary>
    public async Task SetGpoAsync<T>(
        string dirno,
        string gpoId,
        bool active,
        int? timeSeconds,
        CancellationToken ct)
    {
        await SetGpoAsync(dirno, gpoId, active, timeSeconds, ct).ConfigureAwait(false);
    }
}
