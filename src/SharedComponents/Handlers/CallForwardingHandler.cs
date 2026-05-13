using ConnectPro.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wamp.Client;
using Zenitel.IntegrationModule.REST;
using Timer = System.Timers.Timer;

namespace ConnectPro.Handlers
{
    /// <summary>
    /// Handles call forwarding operations, including retrieving, adding, updating, and deleting call forwarding rules.
    /// </summary>
    public class CallForwardingHandler : IDisposable
    {
        private Collections _collections;
        private Events _events;
        private WampClient _wamp;
        private RestClient _rest;
        private object _lockObj = new object();
        private readonly SemaphoreSlim _retrievalGate = new SemaphoreSlim(1, 1);
        private const double CallForwardingReconcileIntervalMs = 5000;
        private Timer CallForwardingRetrievalTimer { get; set; }

        /// <summary>
        /// Indicates whether call forwarding rule retrieval is currently being executed.
        /// </summary>
        public bool IsExecutingRetrieval { get; set; } = false;

        /// <summary>
        /// Gets or sets the IP address of the parent device.
        /// </summary>
        public string ParentIpAddress { get; set; } = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="CallForwardingHandler"/> class.
        /// </summary>
        /// <param name="collections">Reference to the collections object for managing call forwarding rules.</param>
        /// <param name="events">Reference to the events object for triggering call forwarding related events.</param>
        /// <param name="wamp">Reference to the WAMP client for communication.</param>
        /// <param name="rest">Reference to the REST client for API communication.</param>
        /// <param name="parentIpAddress">The IP address of the parent device.</param>
        public CallForwardingHandler(ref Collections collections,
                                     ref Events events,
                                     ref WampClient wamp,
                                     ref RestClient rest,
                                     string parentIpAddress)
        {
            _collections = collections;
            _events = events;
            _wamp = wamp;
            _rest = rest;
            ParentIpAddress = parentIpAddress;

            _events.OnConnectionChanged += HandleConnectionChange;
            InitializeCallForwardingRetrievalTimer();
        }

        /// <summary>
        /// Handles connection status changes. Clears rules on disconnect and subscribes
        /// to the device list change event so rules are retrieved after devices are available.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="isConnected">Indicates whether the connection is established.</param>
        private void HandleConnectionChange(object sender, bool isConnected)
        {
            if (isConnected)
            {
                _events.OnDeviceListChange += HandleDeviceListChange;
                StartCallForwardingRetrievalTimer();
            }
            else
            {
                StopCallForwardingRetrievalTimer();
                _events.OnDeviceListChange -= HandleDeviceListChange;
                _collections.CallForwardingRules.Clear();
                _events.OnCallForwardingRulesChange?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles device list changes. Once devices are available, retrieves call forwarding rules.
        /// </summary>
        private void HandleDeviceListChange(object sender, EventArgs e)
        {
            if (_collections.RegisteredDevices.Count > 0 && _collections.CallForwardingRules.Count == 0)
            {
                Task.Run(async () => await RetrieveCallForwardingRules());
            }
        }

        /// <summary>
        /// Retrieves call forwarding rules from the server for all registered devices.
        /// Iterates over each device individually because the API does not support
        /// retrieving all rules with an empty dirno.
        /// </summary>
        /// <returns>True if retrieval was successful for at least one device; otherwise false.</returns>
        public async Task<bool> RetrieveCallForwardingRules()
        {
            if (!TryEnterGate(_retrievalGate))
                return false;

            try
            {
                var latestRules = await FetchCallForwardingRulesAsync().ConfigureAwait(false);

                if (latestRules == null)
                    return false;

                bool changed;

                lock (_lockObj)
                {
                    var existingRules = _collections.CallForwardingRules?.ToList() ?? new List<CallForwardingRule>();
                    changed = HaveRulesChanged(existingRules, latestRules);

                    if (changed)
                    {
                        _collections.CallForwardingRules.Clear();
                        foreach (var rule in latestRules)
                        {
                            _collections.CallForwardingRules.Add(rule);
                        }
                    }
                }

                if (changed)
                {
                    _events.OnCallForwardingRulesChange?.Invoke(this, EventArgs.Empty);
                }

                return latestRules.Count > 0;
            }
            catch (Exception exe)
            {
                _events.OnExceptionThrown?.Invoke(this, exe);
                return false;
            }
            finally
            {
                ReleaseGate(_retrievalGate);
            }
        }

        /// <summary>
        /// Retrieves call forwarding rules from the server for a specific directory number and forwarding type.
        /// </summary>
        /// <param name="dirno">The directory number to retrieve rules for.</param>
        /// <param name="fwdType">If provided, only return rules of this type. Use empty string for all types.</param>
        /// <returns>True if retrieval was successful; otherwise false.</returns>
        public async Task<bool> RetrieveCallForwardingRules(string dirno, string fwdType)
        {
            if (string.IsNullOrWhiteSpace(dirno))
                return await RetrieveCallForwardingRules();

            if (!TryEnterGate(_retrievalGate))
                return false;

            bool success = false;

            try
            {
                string endpoint = "/api/call_forwarding?dirno=" + Uri.EscapeDataString(dirno);
                if (!string.IsNullOrEmpty(fwdType))
                    endpoint += "&fwd_type=" + Uri.EscapeDataString(fwdType);

                string response = await _rest.GetAsync(endpoint).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(response))
                {
                    var fetchedRules = JsonConvert.DeserializeObject<List<CallForwardingRule>>(response);

                    if (fetchedRules != null)
                    {
                        bool changed = false;

                        lock (_lockObj)
                        {
                            // Get existing rules for comparison
                            var affectedExisting = _collections.CallForwardingRules
                                .Where(r => r.Dirno == dirno &&
                                           (string.IsNullOrEmpty(fwdType) || r.FwdType == fwdType))
                                .ToList();

                            changed = HaveRulesChanged(affectedExisting, fetchedRules);

                            if (changed)
                            {
                                // Remove existing rules for this dirno/fwdType before adding fresh ones
                                _collections.CallForwardingRules.RemoveAll(r =>
                                    r.Dirno == dirno &&
                                    (string.IsNullOrEmpty(fwdType) || r.FwdType == fwdType));

                                foreach (var rule in fetchedRules)
                                {
                                    _collections.CallForwardingRules.Add(rule);
                                }
                            }
                        }

                        if (changed)
                        {
                            _events.OnCallForwardingRulesChange?.Invoke(this, EventArgs.Empty);
                        }

                        success = true;
                    }
                }
            }
            catch (Exception exe)
            {
                _events.OnExceptionThrown?.Invoke(this, exe);
            }
            finally
            {
                ReleaseGate(_retrievalGate);
            }

            return success;
        }

        /// <summary>
        /// Adds or updates a single call forwarding rule.
        /// If a rule with the same dirno and fwd_type exists, it is updated. Otherwise, a new rule is added.
        /// </summary>
        /// <param name="rule">The call forwarding rule to add or update.</param>
        /// <returns>True if the operation succeeded; otherwise false.</returns>
        public async Task<bool> AddOrUpdateCallForwardingRule(CallForwardingRule rule)
        {
            return await AddOrUpdateCallForwardingRules(new List<CallForwardingRule> { rule });
        }

        /// <summary>
        /// Adds or updates multiple call forwarding rules.
        /// </summary>
        /// <param name="rules">The list of call forwarding rules to add or update.</param>
        /// <returns>True if the operation succeeded; otherwise false.</returns>
        public async Task<bool> AddOrUpdateCallForwardingRules(List<CallForwardingRule> rules)
        {
            bool success = false;

            try
            {
                string jsonBody = JsonConvert.SerializeObject(rules);
                string response = await _rest.PostAsync("/api/call_forwarding", jsonBody).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(response))
                {
                    success = true;
                }

                if (success)
                {
                    // Refresh the rules list after successful update
                    await RetrieveCallForwardingRules();
                }
            }
            catch (Exception exe)
            {
                _events.OnExceptionThrown?.Invoke(this, exe);
            }

            return success;
        }

        /// <summary>
        /// Deletes call forwarding rules for the specified directory number and forwarding type.
        /// </summary>
        /// <param name="dirno">Directory number owning the forwarding rules. Use "alldirno" to match all.</param>
        /// <param name="fwdType">The forwarding type to delete (unconditional, on_busy, on_timeout). Use "all" to match all.</param>
        /// <returns>True if the operation succeeded; otherwise false.</returns>
        public async Task<bool> DeleteCallForwardingRules(string dirno, string fwdType)
        {
            bool success = false;

            try
            {
                string endpoint = "/api/call_forwarding?dirno=" + Uri.EscapeDataString(dirno);
                if (!string.IsNullOrEmpty(fwdType))
                    endpoint += "&fwd_type=" + Uri.EscapeDataString(fwdType);

                await _rest.DeleteAsync(endpoint).ConfigureAwait(false);
                success = true;

                if (success)
                {
                    // Refresh the rules list after successful deletion
                    await RetrieveCallForwardingRules();
                }
            }
            catch (Exception exe)
            {
                _events.OnExceptionThrown?.Invoke(this, exe);
            }

            return success;
        }

        /// <summary>
        /// Returns call forwarding rules for a specific directory number from the local collection.
        /// </summary>
        /// <param name="dirno">The directory number to look up.</param>
        /// <returns>A list of call forwarding rules for the specified directory number.</returns>
        public List<CallForwardingRule> GetRulesForDirno(string dirno)
        {
            return _collections.CallForwardingRules
                .Where(r => r.Dirno == dirno)
                .ToList();
        }

        #region Timer Management

        private void InitializeCallForwardingRetrievalTimer()
        {
            if (CallForwardingRetrievalTimer != null)
                return;

            CallForwardingRetrievalTimer = new Timer(CallForwardingReconcileIntervalMs);
            CallForwardingRetrievalTimer.AutoReset = true;
            CallForwardingRetrievalTimer.Elapsed += OnCallForwardingRetrievalTimerElapsed;
        }

        private void StartCallForwardingRetrievalTimer()
        {
            if (CallForwardingRetrievalTimer != null && !CallForwardingRetrievalTimer.Enabled)
            {
                CallForwardingRetrievalTimer.Start();
            }
        }

        private void StopCallForwardingRetrievalTimer()
        {
            if (CallForwardingRetrievalTimer != null && CallForwardingRetrievalTimer.Enabled)
            {
                CallForwardingRetrievalTimer.Stop();
            }
        }

        private async void OnCallForwardingRetrievalTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_disposed || !_wamp.IsConnected)
                return;

            await ReconcileCallForwardingRules().ConfigureAwait(false);
        }

        #endregion

        #region Reconciliation Helpers

        private static bool TryEnterGate(SemaphoreSlim gate)
        {
            try
            {
                return gate.Wait(0);
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        private static void ReleaseGate(SemaphoreSlim gate)
        {
            try
            {
                gate.Release();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SemaphoreFullException)
            {
            }
        }

        /// <summary>
        /// Fetches call forwarding rules from the REST API for all registered devices.
        /// </summary>
        /// <returns>A list of rules if successful; otherwise null.</returns>
        private async Task<List<CallForwardingRule>> FetchCallForwardingRulesAsync()
        {
            try
            {
                var dirnos = _collections.RegisteredDevices
                    .Select(d => d.dirno)
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Distinct()
                    .ToList();

                if (dirnos.Count == 0)
                    return new List<CallForwardingRule>();

                var allRules = new List<CallForwardingRule>();

                foreach (var dirno in dirnos)
                {
                    try
                    {
                        string endpoint = "/api/call_forwarding?dirno=" + Uri.EscapeDataString(dirno);
                        string response = await _rest.GetAsync(endpoint).ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(response))
                        {
                            var rules = JsonConvert.DeserializeObject<List<CallForwardingRule>>(response);
                            if (rules != null)
                            {
                                allRules.AddRange(rules);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _events.OnExceptionThrown?.Invoke(this, ex);
                    }
                }

                return allRules;
            }
            catch (Exception ex)
            {
                _events.OnExceptionThrown?.Invoke(this, ex);
                return null;
            }
        }

        /// <summary>
        /// Generates a stable key for a call forwarding rule.
        /// </summary>
        private static string GetRuleKey(CallForwardingRule rule)
        {
            if (rule == null)
                return string.Empty;

            return (rule.Dirno ?? string.Empty) + "|" + (rule.FwdType ?? string.Empty);
        }

        /// <summary>
        /// Compares two call forwarding rules by their values.
        /// </summary>
        private static bool AreRulesEqual(CallForwardingRule rule1, CallForwardingRule rule2)
        {
            if (rule1 == null && rule2 == null)
                return true;
            if (rule1 == null || rule2 == null)
                return false;

            return string.Equals(rule1.Dirno, rule2.Dirno, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(rule1.FwdType, rule2.FwdType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(rule1.FwdTo, rule2.FwdTo, StringComparison.OrdinalIgnoreCase) &&
                   rule1.Enabled == rule2.Enabled;
        }

        /// <summary>
        /// Determines if two lists of call forwarding rules are effectively different.
        /// </summary>
        private static bool HaveRulesChanged(List<CallForwardingRule> existingRules, List<CallForwardingRule> latestRules)
        {
            if (existingRules == null && latestRules == null)
                return false;
            if (existingRules == null || latestRules == null)
                return true;

            var existingByKey = existingRules
                .Where(r => r != null)
                .GroupBy(GetRuleKey, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var latestByKey = latestRules
                .Where(r => r != null)
                .GroupBy(GetRuleKey, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            if (existingByKey.Count != latestByKey.Count)
                return true;

            foreach (var kvp in latestByKey)
            {
                if (!existingByKey.TryGetValue(kvp.Key, out var existingRule))
                    return true;

                if (!AreRulesEqual(existingRule, kvp.Value))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Periodically fetches call forwarding rules and updates the collection only if changes are detected.
        /// </summary>
        private async Task ReconcileCallForwardingRules()
        {
            if (_disposed || !TryEnterGate(_retrievalGate))
                return;

            try
            {
                if (!_wamp.IsConnected)
                    return;

                var latestRules = await FetchCallForwardingRulesAsync().ConfigureAwait(false);

                if (latestRules == null)
                    return;

                bool changed;

                lock (_lockObj)
                {
                    var existingRules = _collections.CallForwardingRules?.ToList() ?? new List<CallForwardingRule>();
                    changed = HaveRulesChanged(existingRules, latestRules);

                    if (changed)
                    {
                        _collections.CallForwardingRules.Clear();
                        foreach (var rule in latestRules)
                        {
                            _collections.CallForwardingRules.Add(rule);
                        }
                    }
                }

                if (changed)
                {
                    _events.OnCallForwardingRulesChange?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                _events.OnExceptionThrown?.Invoke(this, ex);
            }
            finally
            {
                ReleaseGate(_retrievalGate);
            }
        }

        #endregion

        #region IDisposable Implementation

        private bool _disposed = false;

        /// <summary>
        /// Disposes resources and unsubscribes from events.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources and unsubscribes from events.
        /// </summary>
        /// <param name="disposing">True if called from Dispose; false if from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                StopCallForwardingRetrievalTimer();

                if (CallForwardingRetrievalTimer != null)
                {
                    CallForwardingRetrievalTimer.Elapsed -= OnCallForwardingRetrievalTimerElapsed;
                    CallForwardingRetrievalTimer.Dispose();
                    CallForwardingRetrievalTimer = null;
                }

                if (_events != null)
                {
                    _events.OnConnectionChanged -= HandleConnectionChange;
                    _events.OnDeviceListChange -= HandleDeviceListChange;
                }

                _retrievalGate?.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
