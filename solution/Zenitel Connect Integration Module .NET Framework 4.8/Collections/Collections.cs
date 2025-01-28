using ConnectPro.Models;
using System.Collections.Generic;

namespace ConnectPro
{
    public class Collections
    {
        private List<Device> _registeredDevices;
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
    }
}
