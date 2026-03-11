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
using Timer = System.Timers.Timer;

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
        private const double GroupReconcileIntervalMs = 5000;
        private const double AudioMessageReconcileIntervalMs = 5000;
        private Timer GroupsRetrievalTimer { get; set; }
        private Timer AudioMessagesRetrievalTimer { get; set; }

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
            InitializeGroupsRetrievalTimer();
            InitializeAudioMessagesRetrievalTimer();
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
                StartGroupsRetrievalTimer();
                StartAudioMessagesRetrievalTimer();

                if (_collections.Groups.Count == 0)
                    Task.Run(async () => await RetrieveGroups());
                if (_collections.AudioMessages.Count == 0)
                    Task.Run(async () => await RetrieveAudioMessages());
            }
            else
            {
                StopGroupsRetrievalTimer();
                StopAudioMessagesRetrievalTimer();

                var removedGroups = _collections.Groups.ToList();
                _collections.Groups.Clear();

                var removedMessages = _collections.AudioMessages.ToList();
                _collections.AudioMessages.Clear();

                foreach (var removedMessage in removedMessages)
                    _events.OnAudioMessageRemoved?.Invoke(this, removedMessage);

                if (removedGroups.Count > 0)
                    _events.OnGroupsListChange?.Invoke(this, new EventArgs());

                if (removedMessages.Count > 0)
                    _events.OnAudioMessagesChange?.Invoke(this, false);
            }
        }

        private void OnGroupsRetrievalTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_wamp.IsConnected)
                return;

            Task.Run(async () => await RetrieveGroups().ConfigureAwait(false));
        }

        private void OnAudioMessagesRetrievalTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_wamp.IsConnected)
                return;

            Task.Run(async () => await RetrieveAudioMessages().ConfigureAwait(false));
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

                    bool listChanged;
                    List<Group> addedGroups;
                    List<Group> removedGroups;

                    lock (_lockObj)
                    {
                        var existingGroups = _collections.Groups ?? new List<Group>();

                        var updated = CollectionReconciler.DiffByKey(
                            existingGroups,
                            groups,
                            GetGroupKey,
                            StringComparer.OrdinalIgnoreCase,
                            HasGroupChanged,
                            out addedGroups,
                            out removedGroups);

                        listChanged = addedGroups.Count > 0 || removedGroups.Count > 0 || updated;
                        _collections.Groups = groups;
                    }

                    if (listChanged)
                    {
                        foreach (var addedGroup in addedGroups)
                            _events.OnGroupAdded?.Invoke(this, addedGroup);

                        foreach (var removedGroup in removedGroups)
                            _events.OnGroupRemoved?.Invoke(this, removedGroup);

                        _events.OnGroupsListChange?.Invoke(this, new EventArgs());
                    }
                }
            }
            catch (Exception exe)
            {
                _events.OnDebugChanged?.Invoke(this, (exe.Message, exe));
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
                List<AudioMessage> addedMessages = new List<AudioMessage>();
                List<AudioMessage> removedMessages = new List<AudioMessage>();
                bool listChanged = false;

                lock (_lockObj)
                {
                    if (IsExecutingAudioMessagesRetrieval)
                        return;

                    IsExecutingAudioMessagesRetrieval = true;
                }

                try
                {
                    if (_wamp.IsConnected)
                    try
                    {
                        var latestMessages = GetAudioMessages().AudioMessages?.ToList() ?? new List<AudioMessage>();

                        lock (_lockObj)
                        {
                            var existingMessages = _collections.AudioMessages ?? new List<AudioMessage>();

                            CollectionReconciler.DiffByKey(
                                existingMessages,
                                latestMessages,
                                GetAudioMessageKey,
                                StringComparer.OrdinalIgnoreCase,
                                out addedMessages,
                                out removedMessages);

                            listChanged = addedMessages.Count > 0 || removedMessages.Count > 0;

                            _collections.AudioMessages = latestMessages;
                        }

                        foreach (var addedMessage in addedMessages)
                            _events.OnAudioMessageAdded?.Invoke(this, addedMessage);

                        foreach (var removedMessage in removedMessages)
                            _events.OnAudioMessageRemoved?.Invoke(this, removedMessage);

                        if (listChanged)
                            _events.OnAudioMessagesChange?.Invoke(this, false);
                    }
                    catch (Exception exe)
                    {
                        _events.OnDebugChanged?.Invoke(this, (exe.Message, exe));
                        _events.OnExceptionThrown?.Invoke(this, exe);
                    }
                }
                finally
                {
                    lock (_lockObj)
                    {
                        IsExecutingAudioMessagesRetrieval = false;
                    }
                }
            });
        }

        private static string GetAudioMessageKey(AudioMessage audioMessage)
        {
            if (audioMessage == null)
                return string.Empty;

            return (audioMessage.MessageId.ToString() + "|"
                + (audioMessage.Dirno ?? string.Empty) + "|"
                + (audioMessage.FilePath ?? string.Empty) + "|"
                + (audioMessage.FileName ?? string.Empty))
                .ToLowerInvariant();
        }

        private static string GetGroupKey(Group group)
        {
            if (group == null)
                return string.Empty;

            return (group.Dirno ?? string.Empty).ToLowerInvariant();
        }

        private static bool HasGroupChanged(Group existing, Group latest)
        {
            if (existing == null || latest == null)
                return true;

            if (!string.Equals(existing.Dirno, latest.Dirno, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!string.Equals(existing.DisplayName, latest.DisplayName, StringComparison.Ordinal))
                return true;

            if (!string.Equals(existing.Priority, latest.Priority, StringComparison.Ordinal))
                return true;

            var existingMembers = existing.Members ?? Array.Empty<string>();
            var latestMembers = latest.Members ?? Array.Empty<string>();

            if (existingMembers.Length != latestMembers.Length)
                return true;

            return !existingMembers.SequenceEqual(latestMembers, StringComparer.OrdinalIgnoreCase);
        }

        private void InitializeGroupsRetrievalTimer()
        {
            if (GroupsRetrievalTimer != null)
                return;

            GroupsRetrievalTimer = new Timer(GroupReconcileIntervalMs);
            GroupsRetrievalTimer.AutoReset = true;
            GroupsRetrievalTimer.Elapsed += OnGroupsRetrievalTimerElapsed;
        }

        private void StartGroupsRetrievalTimer()
        {
            if (GroupsRetrievalTimer == null)
                InitializeGroupsRetrievalTimer();

            if (GroupsRetrievalTimer != null && !GroupsRetrievalTimer.Enabled)
                GroupsRetrievalTimer.Start();
        }

        private void StopGroupsRetrievalTimer()
        {
            if (GroupsRetrievalTimer != null && GroupsRetrievalTimer.Enabled)
                GroupsRetrievalTimer.Stop();
        }

        private void InitializeAudioMessagesRetrievalTimer()
        {
            if (AudioMessagesRetrievalTimer != null)
                return;

            AudioMessagesRetrievalTimer = new Timer(AudioMessageReconcileIntervalMs);
            AudioMessagesRetrievalTimer.AutoReset = true;
            AudioMessagesRetrievalTimer.Elapsed += OnAudioMessagesRetrievalTimerElapsed;
        }

        private void StartAudioMessagesRetrievalTimer()
        {
            if (AudioMessagesRetrievalTimer == null)
                InitializeAudioMessagesRetrievalTimer();

            if (AudioMessagesRetrievalTimer != null && !AudioMessagesRetrievalTimer.Enabled)
                AudioMessagesRetrievalTimer.Start();
        }

        private void StopAudioMessagesRetrievalTimer()
        {
            if (AudioMessagesRetrievalTimer != null && AudioMessagesRetrievalTimer.Enabled)
                AudioMessagesRetrievalTimer.Stop();
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

                if (AudioMessagesRetrievalTimer != null)
                {
                    StopAudioMessagesRetrievalTimer();
                    AudioMessagesRetrievalTimer.Elapsed -= OnAudioMessagesRetrievalTimerElapsed;
                    AudioMessagesRetrievalTimer.Dispose();
                    AudioMessagesRetrievalTimer = null;
                }

                if (GroupsRetrievalTimer != null)
                {
                    StopGroupsRetrievalTimer();
                    GroupsRetrievalTimer.Elapsed -= OnGroupsRetrievalTimerElapsed;
                    GroupsRetrievalTimer.Dispose();
                    GroupsRetrievalTimer = null;
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
