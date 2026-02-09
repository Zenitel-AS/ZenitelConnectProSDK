using ConnectPro.Models;
using ConnectPro.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wamp.Client;
using Zenitel.IntegrationModule.REST;

namespace ConnectPro.Handlers
{
    /// <summary>
    /// Handles broadcasting operations, including retrieving groups and audio messages, and playing or stopping audio messages.
    /// </summary>
    public class BroadcastingHandler : IDisposable
    {
        private Collections _collections;
        private Events _events;
        private WampClient _wamp;
        private RestClient _rest;
        private object _lockObj = new object();
        private CancellationTokenSource _playbackCancellationTokenSource;

        /// <summary>
        /// Indicates whether group retrieval is currently being executed.
        /// </summary>
        public bool IsExecutingGroupRetrieval { get; set; } = false;

        /// <summary>
        /// Indicates whether audio message retrieval is currently being executed.
        /// </summary>
        public bool IsExecutingAudioMessagesRetrieval { get; set; } = false;

        /// <summary>
        /// Gets or sets the IP address of the parent device.
        /// </summary>
        public string ParentIpAddress { get; set; } = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="BroadcastingHandler"/> class.
        /// </summary>
        /// <param name="collections">Reference to the collections object for managing groups and audio messages.</param>
        /// <param name="events">Reference to the events object for triggering broadcasting-related events.</param>
        /// <param name="wamp">Reference to the WAMP client for communication.</param>
        /// <param name="rest">Reference to the REST client for fetching group members.</param>
        /// <param name="parentIpAddress">The IP address of the parent device.</param>
        public BroadcastingHandler(ref Collections collections,
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
        /// Handles connection status changes and retrieves groups and audio messages when connected.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="isConnected">Indicates whether the connection is established.</param>
        private void HandleConnectionChange(object sender, bool isConnected)
        {
            if (isConnected)
            {
                if (_collections.Groups.Count == 0)
                    Task.Run(async () => await RetrieveGroups());
                if (_collections.AudioMessages.Count == 0)
                    Task.Run(async () => await RetrieveAudioMessages());
            }
            else
            {
                _collections.Groups.Clear();
                _collections.AudioMessages.Clear();

                _events.OnGroupsListChange?.Invoke(this, new EventArgs());
                _events.OnAudioMessagesChange?.Invoke(this, false);
            }
        }

        /// <summary>
        /// Retrieves groups from the server and updates the groups collection.
        /// Uses WAMP for the initial group list, then REST to fill in members.
        /// </summary>
        public async Task RetrieveGroups()
        {
            lock (_lockObj)
            {
                if (IsExecutingGroupRetrieval)
                    return;
                IsExecutingGroupRetrieval = true;
            }

            try
            {
                if (_wamp.IsConnected)
                {
                    var groups = GetGroups().ToList();
                    await FillGroupMembersFromRestAsync(groups);
                    _collections.Groups = groups;
                    _events.OnGroupsListChange?.Invoke(this, new EventArgs());
                }
            }
            catch (Exception exe)
            {
                _events.OnDebugChanged?.Invoke(this, (exe.Message, exe));
                _events.OnGroupsListChange?.Invoke(this, new EventArgs());
                _events.OnExceptionThrown?.Invoke(this, exe);
            }
            finally
            {
                IsExecutingGroupRetrieval = false;
            }
        }

        /// <summary>
        /// Retrieves groups from the server via WAMP.
        /// </summary>
        /// <returns>A collection of <see cref="Group"/> objects.</returns>
        public IEnumerable<Group> GetGroups()
        {
            return ObjectConverter.ConvertSdkGroupElementList(_wamp.requestGroups("", true).ToList());
        }

        /// <summary>
        /// REST response model matching the /api/groups endpoint JSON shape.
        /// </summary>
        private class RestGroupResponse
        {
            [JsonProperty("call_timeout")]
            public int CallTimeout { get; set; }

            [JsonProperty("dirno")]
            public string Dirno { get; set; }

            [JsonProperty("displayname")]
            public string DisplayName { get; set; }

            [JsonProperty("members")]
            public string[] Members { get; set; }

            [JsonProperty("priority")]
            public int Priority { get; set; }
        }

        /// <summary>
        /// Fills in group members using the REST API for groups that have no members after WAMP retrieval.
        /// </summary>
        /// <param name="groups">The list of groups to fill members for.</param>
        private async Task FillGroupMembersFromRestAsync(List<Group> groups)
        {
            if (_rest == null || groups == null || groups.Count == 0)
                return;

            try
            {
                string response = await _rest.GetAsync("/api/groups?verbose=true").ConfigureAwait(false);
                if (string.IsNullOrEmpty(response))
                    return;

                var restGroups = JsonConvert.DeserializeObject<List<RestGroupResponse>>(response);
                if (restGroups == null)
                    return;

                // Build a lookup by dirno for quick matching
                var memberLookup = new Dictionary<string, string[]>();
                foreach (var rg in restGroups)
                {
                    if (rg.Dirno != null && rg.Members != null && rg.Members.Length > 0)
                    {
                        memberLookup[rg.Dirno] = rg.Members;
                    }
                }

                // Fill in members for groups that are missing them
                foreach (var group in groups)
                {
                    string[] members;
                    if ((group.Members == null || group.Members.Length == 0)
                        && memberLookup.TryGetValue(group.Dirno, out members))
                    {
                        group.Members = members;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail - groups will just have no members
                _events.OnDebugChanged?.Invoke(this, ("REST group members retrieval failed: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// Retrieves audio messages from the server and updates the audio messages collection.
        /// </summary>
        public async Task RetrieveAudioMessages()
        {
            await Task.Run(() =>
            {
                lock (_lockObj)
                {
                    try
                    {
                        if (!IsExecutingAudioMessagesRetrieval)
                        {
                            IsExecutingAudioMessagesRetrieval = true;
                            if (_wamp.IsConnected)
                            {
                                _collections.AudioMessages = GetAudioMessages().AudioMessages.ToList();
                                _events.OnAudioMessagesChange?.Invoke(this, false);
                            }
                            IsExecutingAudioMessagesRetrieval = false;
                        }
                    }
                    catch (Exception exe)
                    {
                        _events.OnDebugChanged?.Invoke(this, (exe.Message, exe));
                        _events.OnAudioMessagesChange?.Invoke(this, false);
                        _events.OnExceptionThrown?.Invoke(this, exe);
                    }
                }
            });
        }

        /// <summary>
        /// Retrieves audio messages from the server.
        /// </summary>
        /// <returns>An <see cref="AudioMessageWrapper"/> containing audio messages.</returns>
        public AudioMessageWrapper GetAudioMessages()
        {
            return ObjectConverter.ConvertSdkAudioMessageWrapper(_wamp.requestAudioMessages());
        }

        /// <summary>
        /// Stops the playback of the specified audio message.
        /// </summary>
        /// <param name="msg">The audio message to stop.</param>
        public void StopAudioMessage(AudioMessage msg)
        {
            _playbackCancellationTokenSource?.Cancel();
            _wamp.DeleteCalls(msg.Dirno);
            msg.IsPlaying = false;
            _events.OnAudioMessagesChange?.Invoke(msg, false);
        }

        /// <summary>
        /// Plays the specified audio message for a given group with optional repeat count.
        /// </summary>
        /// <param name="audioMessage">The audio message to play.</param>
        /// <param name="groupDirno">The directory number of the group.</param>
        /// <param name="action">The action to perform (e.g., \"play\").</param>
        /// <param name="repeatCount">The number of times to repeat the playback. Set to 0 for infinite repeat.</param>
        public async void PlayAudioMessage(AudioMessage audioMessage, string groupDirno, string action, int repeatCount)
        {
            if (audioMessage == null) return;

            _playbackCancellationTokenSource?.Cancel();
            _playbackCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _playbackCancellationTokenSource.Token;

            _events.OnAudioMessagesChange?.Invoke(audioMessage, true);
            audioMessage.IsPlaying = true;

            try
            {
                do
                {
                    _wamp.PostCalls(audioMessage.Dirno, groupDirno, action);
                    await WaitForMessageDuration(audioMessage, cancellationToken);

                    if (repeatCount > 0) repeatCount--;
                }
                while (repeatCount != 0 && !cancellationToken.IsCancellationRequested);
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation gracefully
            }
            finally
            {
                audioMessage.IsPlaying = false;
                _events.OnAudioMessagesChange?.Invoke(audioMessage, false);
            }
        }

        /// <summary>
        /// Waits for the duration of the specified audio message before continuing.
        /// </summary>
        /// <param name="msg">The audio message.</param>
        /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
        private async Task WaitForMessageDuration(AudioMessage msg, CancellationToken cancellationToken)
        {
            if (msg.Duration.HasValue)
            {
                TimeSpan duration = TimeSpan.FromSeconds(msg.Duration.Value + 2);
                await Task.Delay(duration, cancellationToken);
            }
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
        /// Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <remarks>This method is called by public <c>Dispose()</c> methods and the finalizer, if
        /// implemented. Override this method to release resources specific to derived classes. Always call the base
        /// class implementation when overriding.</remarks>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release
        /// only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Unsubscribe from events
                if (_events != null)
                {
                    _events.OnConnectionChanged -= HandleConnectionChange;
                }

                // Dispose CancellationTokenSource
                if (_playbackCancellationTokenSource != null)
                {
                    _playbackCancellationTokenSource.Cancel();
                    _playbackCancellationTokenSource.Dispose();
                    _playbackCancellationTokenSource = null;
                }
            }

            _disposed = true;
        }

        #endregion

    }
}
