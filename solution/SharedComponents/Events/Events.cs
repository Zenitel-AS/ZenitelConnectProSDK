using ConnectPro.Models;
using ConnectPro.Models.AudioAnalytics;
using System;

namespace ConnectPro
{
    public class Events
    {
        private AudioAnalytics _audioAnalytics;
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
        /// Occurs when the value of the active call list changes.
        /// </summary>
        public EventHandler OnActiveCallListValueChange { get; set; }

        /// <summary>
        /// Occurs when the queued call list undergoes changes.
        /// </summary>
        public EventHandler OnCallQueueListValueChange { get; set; }

        /// <summary>
        /// Occurs when debugging information is modified or added.
        /// </summary>
        public EventHandler<(string, object)> OnDebugChanged { get; set; }

        /// <summary>
        /// Signals the initiation of building a log entry, not yet saved.
        /// </summary>
        public EventHandler<object> OnLogEntryRequested { get; set; }

        /// <summary>
        /// Occurs after a log entry has been successfully added.
        /// </summary>
        public EventHandler<CallLog> OnLogEntryAdded { get; set; }

        /// <summary>
        /// Signals the start of the device retrieval procedure.
        /// </summary>
        public EventHandler OnDeviceRetrievalStart { get; set; }

        /// <summary>
        /// Signals the end of the device retrieval procedure.
        /// </summary>
        public EventHandler OnDeviceRetrievalEnd { get; set; }

        /// <summary>
        /// Used to signal the CallHandler to retrieve all active and queued calls.
        /// </summary>
        public EventHandler OnQueuesAndCallsSync { get; set; }

        /// <summary>
        /// Used to gracefully handle exceptions, creating an ErrorLog entry when an exception occurs.
        /// </summary>
        public EventHandler<Exception> OnExceptionThrown { get; set; }

        /// <summary>
        /// Occurs when the connection to Zenitel Connect changes.
        /// </summary>
        public EventHandler<bool> OnConnectionChanged { get; set; }

        /// <summary>
        /// Signals a change in configuration data.
        /// </summary>
        public EventHandler<ConnectPro.Configuration> OnConfigurationChanged { get; set; }

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
        /// Occurs on Group broadcast update 
        /// </summary>
        public EventHandler<object> OnGroupsMsgUpdate { get; set; }

        /// <summary>
        /// Occurs on open door event
        /// </summary>
        public EventHandler<bool> OnDoorOpen { get; set; }

        /// <summary>
        /// Occurs on audio message update
        /// </summary>
        public EventHandler<bool> OnAudioMessagesChange { get; set; }

        //public EventHandler<AudioEvent> OnAudioEventDetected { get; set; }
        //public EventHandler<AudioEventData> OnAudioEventDataRecieved { get; set; }
        //public EventHandler<AudioDetectorAliveSignal> OnAudioDetectorAliveSignal { get; set; }
    }
    public class AudioAnalytics
    {
        public EventHandler<DataReceived> DataReceived { get; set; }
        public EventHandler<Heartbeat> Heartbeat { get; set; }
        public EventHandler<AudioEventDetected> AudioEventDetected { get; set; }

    }
}
