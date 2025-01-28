using ConnectPro.Models;
using System.Collections.Generic;

namespace ConnectPro
{
    public class Collections
    {
        private List<Device> _registeredDevices;
        /// <summary>
        /// Gets or sets the collection of all Zenitel devices registered with Zenitel Connect.
        /// </summary>
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
            set
            {
                _registeredDevices = value;
            }
        }

        private List<Device> _activeCalls;
        /// <summary>
        /// Gets or sets the collection of all currently active calls.
        /// </summary>
        public List<Device> ActiveCalls
        {
            get
            {
                if (_activeCalls == null)
                    _activeCalls = new List<Device>();
                return _activeCalls;
            }
            set
            {
                _activeCalls = value;
            }
        }

        private List<CallLegElement> _callQueue;
        /// <summary>
        /// Gets or sets the collection of all currently queued calls.
        /// </summary>
        public List<CallLegElement> CallQueue
        {
            get
            {
                if (_callQueue == null)
                    _callQueue = new List<CallLegElement>();
                return _callQueue;
            }
            set
            {
                _callQueue = value;
            }
        }

        private List<Group> _groups;
        /// <summary>
        /// Gets or sets the collection of all groups.
        /// </summary>
        public List<Group> Groups
        {
            get
            {
                if (_groups == null)
                    _groups = new List<Group>();
                return _groups;
            }
            set { _groups = value; }
        }

        private List<AudioMessage> _audioMessages;
        /// <summary>
        /// Gets or sets the collection of all audio messages.
        /// </summary>
        public List<AudioMessage> AudioMessages
        {
            get
            {
                if (_audioMessages == null)
                    _audioMessages = new List<AudioMessage>();
                return _audioMessages;
            }
            set { _audioMessages = value; }
        }

        private List<DirectoryNumber> _directoryNumbers;
        /// <summary>
        /// Gets or sets the collection of all audio messages.
        /// </summary>
        public List<DirectoryNumber> DirectoryNumbers
        {
            get
            {
                if (_directoryNumbers == null)
                    _directoryNumbers = new List<DirectoryNumber>();
                return _directoryNumbers;
            }
            set { _directoryNumbers = value; }
        }
    }
}
