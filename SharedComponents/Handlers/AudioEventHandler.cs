using ConnectPro.Models;
using ConnectPro.Models.AudioAnalytics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Wamp.Client;
using static Wamp.Client.WampClient;

namespace ConnectPro.Handlers
{
    public class AudioEventHandler
    {
        Events _events;
        WampClient _wamp;

        public string ParentIpAddress { get; set; } = "";

        public AudioEventHandler(ref Events events, ref WampClient wamp, string parentIpAddress)
        {
            ParentIpAddress = parentIpAddress;

            _events = events;
            _wamp = wamp;

            _wamp.OnAudioEventDetection += HandleAudioEventDetection;
            _wamp.OnAudioDataReceiving += HandleAudioDataRecievingEvent;
            _wamp.OnAudioDetectorAlive += HandleAudioDataAliveEvent;
        }

        private void HandleAudioEventDetection(object sender, wamp_audio_event_detection audioEvent)
        {
            AudioEventDetected _audioEvent = new AudioEventDetected(audioEvent);

            _events.AudioAnalytics.AudioEventDetected?.Invoke(this, _audioEvent);
        }

        private void HandleAudioDataRecievingEvent(object sender, wamp_audio_data_receiving audioData)
        {
            DataReceived audioEventData = new DataReceived(audioData);

            _events.AudioAnalytics.DataReceived?.Invoke(this, audioEventData);
        }

        private void HandleAudioDataAliveEvent(object sender, wamp_audio_detector_alive aliveSignal)
        {
            Heartbeat signal = new Heartbeat(aliveSignal);

            _events.AudioAnalytics.Heartbeat?.Invoke(this, signal);
        }
    }
}
