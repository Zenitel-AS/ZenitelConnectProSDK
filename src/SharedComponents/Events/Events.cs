using ConnectPro.Models;
using ConnectPro.Models.AudioAnalytics;
using System;

namespace ConnectPro
{
    /// <summary>
    /// Defines event handlers used throughout the system for various state changes and notifications.
    /// </summary>
    public class Events
    {
        #region Fields

        private AudioAnalytics _audioAnalytics;

        #endregion

        #region Properties

        /// <summary>
        /// Provides access to the audio analytics event handlers.
        /// </summary>
        public AudioAnalytics AudioAnalytics
        {
            get
            {
                if (_audioAnalytics == null)
                    _audioAnalytics = new AudioAnalytics();
                return _audioAnalytics;
            }
            set
            {
                _audioAnalytics = value;
            }
        }

        #endregion

        #region Device Events

        /// <summary>
        /// Occurs when a change in the active camera feed is detected.
        /// </summary>
        public EventHandler OnActiveVideoFeedChange { get; set; }

        /// <summary>
        /// Occurs when the operator directory number is updated.
        /// </summary>
        public EventHandler<string> OnOperatorDirNoChange { get; set; }

        /// <summary>
        /// Occurs when the device list undergoes modifications.
        /// </summary>
        public EventHandler OnDeviceListChange { get; set; }

        /// <summary>
        /// Occurs when the state of a device in the list changes.
        /// </summary>
        public EventHandler<CallElement> OnDeviceStateChange { get; set; }

        /// <summary>
        /// Occurs when a Device <seealso cref="Enums.CallState"/> CallState undergoes changes.
        /// </summary>
        public EventHandler<Device> OnDeviceCallStateChange { get; set; }

        #endregion

        #region Call Events

        /// <summary>
        /// Occurs when the value of the active call list changes.
        /// </summary>
        public EventHandler OnActiveCallListValueChange { get; set; }

        /// <summary>
        /// Occurs when the queued call list undergoes changes.
        /// </summary>
        public EventHandler OnCallQueueListValueChange { get; set; }

        /// <summary>
        /// Occurs when a call manipulation has been reported from wamp.
        /// </summary>
        public EventHandler<CallElement> OnCallEvent { get; set; }

        #endregion

        #region Logging & Debugging Events

        /// <summary>
        /// Occurs when debugging information is modified or added.
        /// </summary>
        public EventHandler<(string, object)> OnDebugChanged { get; set; }

        /// <summary>
        /// Signals the initiation of building a log entry, not yet saved.
        /// </summary>
        public EventHandler<object> OnCallLogEntryRequested { get; set; }

        /// <summary>
        /// Occurs after a log entry has been successfully added.
        /// </summary>
        public EventHandler<CallLog> OnCallLogEntryAdded{ get; set; }
        /// <summary>
        /// Occurs when a child log entry is added in WAMP.
        /// </summary>
        public EventHandler<string> OnChildLogEntry { get; set; }

        #endregion

        #region Device Retrieval Events

        /// <summary>
        /// Signals the start of the device retrieval procedure.
        /// </summary>
        public EventHandler OnDeviceRetrievalStart { get; set; }

        /// <summary>
        /// Signals the end of the device retrieval procedure.
        /// </summary>
        public EventHandler OnDeviceRetrievalEnd { get; set; }

        /// <summary>
        /// Signals the result of the device test procedure (tone or button).
        /// </summary>
        public EventHandler<ExtendedStatus> OnDeviceTest { get; set; }

        #endregion

        #region System & Configuration Events

        /// <summary>
        /// Used to signal the CallHandler to retrieve all active and queued calls.
        /// </summary>
        public EventHandler OnQueuesAndCallsSync { get; set; }

        /// <summary>
        /// Used to gracefully handle exceptions, creating an error log entry when an exception occurs.
        /// </summary>
        public EventHandler<Exception> OnExceptionThrown { get; set; }

        /// <summary>
        /// Occurs when the connection to Zenitel Connect changes.
        /// </summary>
        public EventHandler<bool> OnConnectionChanged { get; set; }

        /// <summary>
        /// Signals a change in configuration data.
        /// </summary>
        public EventHandler<Configuration> OnConfigurationChanged { get; set; }

        #endregion

        #region UI & Interaction Events

        /// <summary>
        /// Used to open/close a popup window.
        /// </summary>
        public EventHandler<bool> CallHandlerPopupRequested { get; set; }

        /// <summary>
        /// Occurs when manually switching the camera feed is requested.
        /// </summary>
        public EventHandler<object> OnManualVideoFeedChange { get; set; }

        /// <summary>
        /// Occurs on changes to the Group list.
        /// </summary>
        public EventHandler OnGroupsListChange { get; set; }

        /// <summary>
        /// Occurs when a group broadcast update is received.
        /// </summary>
        public EventHandler<object> OnGroupsMsgUpdate { get; set; }

        /// <summary>
        /// Occurs when a door open event is detected.
        /// </summary>
        public EventHandler<bool> OnDoorOpen { get; set; }

        /// <summary>
        /// Occurs when an audio message is updated.
        /// </summary>
        public EventHandler<bool> OnAudioMessagesChange { get; set; }

        #endregion
    }

    /// <summary>
    /// Defines event handlers for audio analytics operations.
    /// </summary>
    public class AudioAnalytics
    {
        /// <summary>
        /// Occurs when the Audio Event Detector receives data.
        /// </summary>
        public EventHandler<DataReceived> DataReceived { get; set; }

        /// <summary>
        /// Occurs when the Audio Event Detector sends a heartbeat signal every minute.
        /// </summary>
        public EventHandler<Heartbeat> Heartbeat { get; set; }

        /// <summary>
        /// Occurs when the Audio Event Detector detects an audio event.
        /// </summary>
        public EventHandler<AudioEventDetected> AudioEventDetected { get; set; }
    }
}
