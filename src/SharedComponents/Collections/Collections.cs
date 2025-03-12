using ConnectPro.Models;
using System.Collections.Generic;

namespace ConnectPro
{
    /// <summary>
    /// Manages collections of devices, active calls, queued calls, groups, 
    /// audio messages, and directory numbers used within Zenitel Connect.
    /// </summary>
    public class Collections
    {
        #region Fields

        private List<Device> _registeredDevices;
        private List<Device> _activeCalls;
        private List<CallLegElement> _callQueue;
        private List<CallElement> _callList;
        private List<Group> _groups;
        private List<AudioMessage> _audioMessages;
        private List<DirectoryNumber> _directoryNumbers;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the collection of all Zenitel devices registered with Zenitel Connect.
        /// </summary>
        /// <remarks>
        /// This list is initialized lazily to ensure efficient memory usage.
        /// </remarks>
        public List<Device> RegisteredDevices
        {
            get
            {
                if (_registeredDevices == null)
                {
                    _registeredDevices = new List<Device>();
                }
                return _registeredDevices;
            }
            set { _registeredDevices = value; }
        }

        /// <summary>
        /// Gets or sets the collection of all currently active calls.
        /// </summary>
        /// <remarks>
        /// This list contains active calls and is updated dynamically 
        /// as calls are initiated or ended.
        /// </remarks>
        public List<Device> ActiveCalls
        {
            get
            {
                if (_activeCalls == null)
                {
                    _activeCalls = new List<Device>();
                }
                return _activeCalls;
            }
            set { _activeCalls = value; }
        }

        /// <summary>
        /// Gets or sets the collection of all currently queued calls.
        /// </summary>
        /// <remarks>
        /// Queued calls are waiting to be answered or processed.
        /// </remarks>
        public List<CallLegElement> CallQueue
        {
            get
            {
                if (_callQueue == null)
                {
                    _callQueue = new List<CallLegElement>();
                }
                return _callQueue;
            }
            set { _callQueue = value; }
        }
        /// <summary>
        /// Gets or sets the collection of all calls.
        /// </summary>

        public List<CallElement> CallList
        {
            get
            {
                if (_callList == null)
                {
                    _callList = new List<CallElement>();
                }
                return _callList;
            }
            set { _callList = value; }
        }

        /// <summary>
        /// Gets or sets the collection of all groups defined in Zenitel Connect Pro.
        /// </summary>
        /// <remarks>
        /// This includes user-defined or system-defined groups used for 
        /// organizing devices or managing communication.
        /// </remarks>
        public List<Group> Groups
        {
            get
            {
                if (_groups == null)
                {
                    _groups = new List<Group>();
                }
                return _groups;
            }
            set { _groups = value; }
        }

        /// <summary>
        /// Gets or sets the collection of all audio messages uploaded to Zenitel Connect Pro.
        /// </summary>
        /// <remarks>
        /// These messages may include system notifications, alerts, or pre-recorded messages.
        /// </remarks>
        public List<AudioMessage> AudioMessages
        {
            get
            {
                if (_audioMessages == null)
                {
                    _audioMessages = new List<AudioMessage>();
                }
                return _audioMessages;
            }
            set { _audioMessages = value; }
        }

        /// <summary>
        /// Gets or sets the collection of all directory numbers.
        /// </summary>
        /// <remarks>
        /// Directory numbers are used for identifying and managing communication endpoints.
        /// </remarks>
        public List<DirectoryNumber> DirectoryNumbers
        {
            get
            {
                if (_directoryNumbers == null)
                {
                    _directoryNumbers = new List<DirectoryNumber>();
                }
                return _directoryNumbers;
            }
            set { _directoryNumbers = value; }
        }

        #endregion
    }
}
