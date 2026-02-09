using ConnectPro.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wamp.Client;
using Zenitel.IntegrationModule.REST;

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
            }
            else
            {
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
            lock (_lockObj)
            {
                if (IsExecutingRetrieval)
                    return false;
                IsExecutingRetrieval = true;
            }

            bool success = false;

            try
            {
                var dirnos = _collections.RegisteredDevices
                    .Select(d => d.dirno)
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Distinct()
                    .ToList();

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
                                success = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _events.OnExceptionThrown?.Invoke(this, ex);
                    }
                }

                _collections.CallForwardingRules.Clear();
                foreach (var rule in allRules)
                {
                    _collections.CallForwardingRules.Add(rule);
                }

                _events.OnCallForwardingRulesChange?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exe)
            {
                _events.OnExceptionThrown?.Invoke(this, exe);
            }
            finally
            {
                lock (_lockObj)
                {
                    IsExecutingRetrieval = false;
                }
            }

            return success;
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

            lock (_lockObj)
            {
                if (IsExecutingRetrieval)
                    return false;
                IsExecutingRetrieval = true;
            }

            bool success = false;

            try
            {
                string endpoint = "/api/call_forwarding?dirno=" + Uri.EscapeDataString(dirno);
                if (!string.IsNullOrEmpty(fwdType))
                    endpoint += "&fwd_type=" + Uri.EscapeDataString(fwdType);

                string response = await _rest.GetAsync(endpoint).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(response))
                {
                    var rules = JsonConvert.DeserializeObject<List<CallForwardingRule>>(response);

                    if (rules != null)
                    {
                        // Remove existing rules for this dirno/fwdType before adding fresh ones
                        _collections.CallForwardingRules.RemoveAll(r =>
                            r.Dirno == dirno &&
                            (string.IsNullOrEmpty(fwdType) || r.FwdType == fwdType));

                        foreach (var rule in rules)
                        {
                            _collections.CallForwardingRules.Add(rule);
                        }

                        success = true;
                    }
                }

                _events.OnCallForwardingRulesChange?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception exe)
            {
                _events.OnExceptionThrown?.Invoke(this, exe);
            }
            finally
            {
                lock (_lockObj)
                {
                    IsExecutingRetrieval = false;
                }
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
                if (_events != null)
                {
                    _events.OnConnectionChanged -= HandleConnectionChange;
                    _events.OnDeviceListChange -= HandleDeviceListChange;
                }
            }

            _disposed = true;
        }

        #endregion
    }
}
