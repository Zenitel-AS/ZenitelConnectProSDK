using ConnectPro.Models;
using ConnectPro.Models.AudioAnalytics;
using Newtonsoft.Json;
using System;
using Wamp.Client;
using static Wamp.Client.WampClient;

namespace ConnectPro.Handlers
{
    /// <summary>
    /// Handles audio events from the WAMP client, including detection, data receiving, and detector alive signals.
    /// </summary>
    public class AudioEventHandler
    {
        #region Fields

        private readonly Events _events;
        private readonly WampClient _wamp;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the IP address of the parent device.
        /// </summary>
        public string ParentIpAddress { get; set; } = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioEventHandler"/> class.
        /// </summary>
        /// <param name="events">Reference to the events object for triggering analytics events.</param>
        /// <param name="wamp">Reference to the WAMP client for handling audio events.</param>
        /// <param name="parentIpAddress">IP address of the parent device.</param>
        public AudioEventHandler(ref Events events, ref WampClient wamp, string parentIpAddress)
        {
            ParentIpAddress = parentIpAddress;
            _events = events;
            _wamp = wamp;

            // Subscribe to WAMP audio event handlers
            _wamp.OnAudioEventDetection += HandleAudioEventDetection;
            _wamp.OnAudioDataReceiving += HandleAudioDataReceivingEvent;
            _wamp.OnAudioDetectorAlive += HandleAudioDetectorAliveEvent;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles audio event detection and triggers the <see cref="AudioAnalytics.AudioEventDetected"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="audioEvent">The detected audio event details.</param>
        private void HandleAudioEventDetection(object sender, wamp_audio_event_detection audioEvent)
        {
            AudioEventDetected detectedEvent = new AudioEventDetected(audioEvent);

            // Invoke the AudioEventDetected event
            _events.AudioAnalytics.AudioEventDetected?.Invoke(this, detectedEvent);
        }

        /// <summary>
        /// Handles audio data receiving and triggers the <see cref="AudioAnalytics.DataReceived"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="audioData">The received audio data details.</param>
        private void HandleAudioDataReceivingEvent(object sender, wamp_audio_data_receiving audioData)
        {
            DataReceived receivedData = new DataReceived(audioData);

            // Invoke the DataReceived event
            _events.AudioAnalytics.DataReceived?.Invoke(this, receivedData);
        }

        /// <summary>
        /// Handles detector alive signals and triggers the <see cref="AudioAnalytics.Heartbeat"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="aliveSignal">The detector alive signal details.</param>
        private void HandleAudioDetectorAliveEvent(object sender, wamp_audio_detector_alive aliveSignal)
        {
            Heartbeat heartbeatSignal = new Heartbeat(aliveSignal);

            // Invoke the Heartbeat event
            _events.AudioAnalytics.Heartbeat?.Invoke(this, heartbeatSignal);
        }

        #endregion
    }
}
