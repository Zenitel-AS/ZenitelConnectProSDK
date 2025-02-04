using ConnectPro.Models;
using ConnectPro.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wamp.Client;

namespace ConnectPro.Handlers
{
    /// <summary>
    /// Handles broadcasting operations, including retrieving groups and audio messages, and playing or stopping audio messages.
    /// </summary>
    public class BroadcastingHandler
    {
        private Collections _collections;
        private Events _events;
        private WampClient _wamp;
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
        /// <param name="parentIpAddress">The IP address of the parent device.</param>
        public BroadcastingHandler(ref Collections collections,
                                   ref Events events,
                                   ref WampClient wamp,
                                   string parentIpAddress)
        {
            _collections = collections;
            _events = events;
            _wamp = wamp;
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
        /// </summary>
        public async Task RetrieveGroups()
        {
            await Task.Run(() =>
            {
                lock (_lockObj)
                {
                    try
                    {
                        if (!IsExecutingGroupRetrieval)
                        {
                            IsExecutingGroupRetrieval = true;
                            if (_wamp.IsConnected)
                            {
                                _collections.Groups = GetGroups().ToList();
                                _events.OnGroupsListChange?.Invoke(this, new EventArgs());
                            }
                            IsExecutingGroupRetrieval = false;
                        }
                    }
                    catch (Exception exe)
                    {
                        _events.OnDebugChanged?.Invoke(this, (exe.Message, exe));
                        _events.OnGroupsListChange?.Invoke(this, new EventArgs());
                        _events.OnExceptionThrown?.Invoke(this, exe);
                    }
                }
            });
        }

        /// <summary>
        /// Retrieves groups from the server.
        /// </summary>
        /// <returns>A collection of <see cref="Group"/> objects.</returns>
        public IEnumerable<Group> GetGroups()
        {
            return ObjectConverter.ConvertSdkGroupElementList(_wamp.requestGroups("", true).ToList());
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
    }
}
