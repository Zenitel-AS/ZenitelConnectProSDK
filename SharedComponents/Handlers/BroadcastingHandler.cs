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
    public class BroadcastingHandler
    {
        private Collections _collections;
        private Events _events;
        private WampClient _wamp;
        private object _lockObj = new object();
        private CancellationTokenSource _playbackCancellationTokenSource;

        public bool IsExecutingGroupRetrieval { get; set; } = false;
        public bool IsExecutingAudioMessagesRetrieval { get; set; } = false;
        public string ParentIpAddress { get; set; } = "";

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
        public IEnumerable<Group> GetGroups()
        {
            return ObjectConverter.ConvertSdkGroupElementList(_wamp.requestGroups("", true).ToList());
        }

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
        public AudioMessageWrapper GetAudioMessages()
        {
            return ObjectConverter.ConvertSdkAudioMessageWrapper(_wamp.requestAudioMessages());
        }

        public void StopAudioMessage(AudioMessage msg)
        {
            _playbackCancellationTokenSource?.Cancel(); // Cancel the ongoing playback
            _wamp.DeleteCalls(msg.Dirno); // Stop the current message
            msg.IsPlaying = false; // Update the playing status
            _events.OnAudioMessagesChange?.Invoke(msg, false); // Notify that playback has stopped
        }


        public async void PlayAudioMessage(AudioMessage audioMessage, string groupDirno, string action, int repeatCount)
        {
            if (audioMessage == null) return;

            _playbackCancellationTokenSource?.Cancel(); // Cancel any ongoing playback
            _playbackCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _playbackCancellationTokenSource.Token;

            _events.OnAudioMessagesChange?.Invoke(audioMessage, true); // Notify that playback has started
            audioMessage.IsPlaying = true;

            try
            {
                do
                {
                    _wamp.PostCalls(audioMessage.Dirno, groupDirno, action); // Play the audio message
                    await WaitForMessageDuration(audioMessage, cancellationToken); // Wait for the message to finish playing

                    if (repeatCount > 0) repeatCount--; // Decrease the repeat count, if applicable
                }
                while (repeatCount != 0 && !cancellationToken.IsCancellationRequested); // Check cancellation
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation gracefully if needed
            }
            finally
            {
                audioMessage.IsPlaying = false; // Mark the message as not playing
                _events.OnAudioMessagesChange?.Invoke(audioMessage, false); // Notify that playback has stopped
            }
        }


        private async Task WaitForMessageDuration(AudioMessage msg, CancellationToken cancellationToken)
        {
            if (msg.Duration.HasValue)
            {
                TimeSpan duration = TimeSpan.FromSeconds(msg.Duration.Value + 2);
                await Task.Delay(duration, cancellationToken); // Asynchronously wait with cancellation support
            }
        }
    }
}
