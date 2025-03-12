using ConnectPro.Models;
using ConnectPro.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Wamp.Client.WampClient;
using Wamp.Client;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections;
using System.Runtime.CompilerServices;
using ConnectPro.Enums;
using System.Collections.ObjectModel;
using System.Net;

namespace ConnectPro.Handlers
{
    /// <summary>
    /// Manages call handling operations, including active and queued calls, 
    /// interactions with the WAMP client, and synchronization of call data.
    /// </summary>
   
    public class CallHandler : IDisposable
    {
        #region Fields & Locks

        /// <summary>
        /// Stores a reference to the collections object, managing calls and devices.
        /// </summary>
     
        private Collections _collections;
      
        /// <summary>
        /// Stores a reference to the events object, handling various call-related events.
        /// </summary>
      
        private Events _events;
      
        /// <summary>
        /// Stores a reference to the WAMP client, enabling communication with the system.
        /// </summary>
      
        private WampClient _wamp;
      
        /// <summary>
        /// Stores a reference to the configuration object, maintaining, connection information, operator, and device settings.
        /// </summary>
      
        private Configuration _configuration;
       
        /// <summary>
        /// Stores the directory number of the operator.
        /// </summary>
       
        private string _operatorDirno;

        private bool IsHandlingCallStatusChange { get; set; } = false;
      
        /// <summary>
        /// Lock object to synchronize posting new calls.
        /// </summary>
      
        private readonly object _postCallLock = new object();
     
        /// <summary>
        /// Lock object to synchronize deletion of all calls.
        /// </summary>
      
        private readonly object _deleteAllCallLock = new object();
      
        /// <summary>
        /// Lock object to synchronize deletion of a specific call.
        /// </summary>
      
        private readonly object _deleteCallLock = new object();
      
        /// <summary>
        /// Lock object to synchronize adding a call to the active calls list.
        /// </summary>
     
        private readonly object _addToActiveCallsLock = new object();
      
        /// <summary>
        /// Lock object to synchronize removing a call from the active calls list.
        /// </summary>
      
        private readonly object _removeFromActiveCallLock = new object();
      
        /// <summary>
        /// Lock object to synchronize answering a queued call.
        /// </summary>
      
        private readonly object _answerQueuedCallLock = new object();
      
        /// <summary>
        /// Lock object to synchronize adding a call to the queued calls list.
        /// </summary>
      
        private readonly object _addToQueuedCallsLock = new object();
      
        /// <summary>
        /// Lock object to synchronize removing a call from the queued calls list.
        /// </summary>
      
        private readonly object _removeFromQueuedCallsLock = new object();
      
        /// <summary>
        /// Lock object to synchronize retrieving all calls and queued calls.
        /// </summary>
       
        private readonly object _getAllCalsAndQueuesLock = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the IP address of the parent device.
        /// </summary>
       
        public string ParentIpAddress { get; set; } = "";
       
        /// <summary>
        /// Gets the operator device based on the operator's directory number.
        /// </summary>
       
        public Device Operator
        {
            get
            {
                if (String.IsNullOrEmpty(_operatorDirno))
                {
                    return null;
                }
                else if (_collections != null)
                {
                    return _collections.RegisteredDevices.FirstOrDefault(x => (x.dirno == _operatorDirno));
                }
                return null;
            }
        }
       
        /// <summary>
        /// Gets or sets the currently active device.
        /// </summary>
      
        public Device ActiveDevice { get; set; }

        #endregion

        #region Constructor & Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="CallHandler"/> class.
        /// </summary>
        /// <param name="collections">Reference to the collections object for managing calls and devices.</param>
        /// <param name="events">Reference to the events object for handling call-related events.</param>
        /// <param name="wamp">Reference to the WAMP client for communication.</param>
        /// <param name="configuration">Reference to the configuration object.</param>
        /// <param name="parentIpAddress">The IP address of the parent device.</param>
       
        public CallHandler(ref Collections collections, ref Events events, ref WampClient wamp, ref Configuration configuration, string parentIpAddress)
        {
            _collections = collections;
            _events = events;
            _wamp = wamp;
            _configuration = configuration;
            _operatorDirno = _configuration.OperatorDirNo;

            this.ParentIpAddress = parentIpAddress;

            _wamp.OnWampCallStatusEvent += HandleCallStatusChangedEvent;
            _wamp.OnWampCallLegStatusEvent += HandleCallQueueChange;

            _events.OnQueuesAndCallsSync += HandleQueuesAndCallsSync;
            _events.OnOperatorDirNoChange += HandleOperatorDirectoryNumberChange;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles changes in the call status and updates active calls or queues accordingly.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="call_Element">The call element containing the updated call information.</param>
       
        private void HandleCallStatusChangedEvent(object sender, WampClient.wamp_call_element call_Element)
        {
            CallElement callElement = new CallElement(call_Element);
            callElement.StartTime = DateTime.UtcNow;
            _events.OnCallLogEntryRequested?.Invoke(this, callElement);
            _events.OnCallEvent?.Invoke(this, callElement);

            if (this.IsHandlingCallStatusChange == false)
            {
                this.IsHandlingCallStatusChange = true;
                Device device;

                if (_collections.RegisteredDevices.Count == 0)
                {
                    this.IsHandlingCallStatusChange = false;
                    return;
                }

                if (callElement.FromDirno == _configuration.OperatorDirNo)
                    device = _collections.RegisteredDevices.FirstOrDefault(rd => rd.dirno == callElement.ToDirno);
                else
                    device = _collections.RegisteredDevices.FirstOrDefault(rd => rd.dirno == callElement.FromDirno);

                if (device == null)
                {
                    this.IsHandlingCallStatusChange = false;
                    return;
                }

                switch (callElement.CallState)
                {
                    case CallState.in_call:
                        if (callElement.ToDirnoCurrent == _configuration.OperatorDirNo || callElement.FromDirno == _configuration.OperatorDirNo)
                            Task.Run(async () => await AddToActiveCalls(device));
                        _events.OnDeviceStateChange?.Invoke(this, callElement);
                        _events.OnActiveVideoFeedChange?.Invoke(this, EventArgs.Empty);
                        this.IsHandlingCallStatusChange = false;
                        break;
                    case CallState.ended:
                        Device activeDevice = _collections.ActiveCalls.Find(ac =>
                                        ac.dirno == callElement.FromDirno
                                        || ac.dirno == callElement.ToDirno);

                        if (activeDevice != null)
                        {
                            RemoveFromActiveCall(activeDevice);
                        }
                        _events.OnDeviceStateChange?.Invoke(this, callElement);
                        this.IsHandlingCallStatusChange = false;

                        break;
                    case CallState.queued:
                    case CallState.ringing:
                        _events.OnDeviceStateChange?.Invoke(this, callElement);
                        _events.OnActiveVideoFeedChange?.Invoke(this, EventArgs.Empty);
                        this.IsHandlingCallStatusChange = false;
                        break;
                    default:
                        this.IsHandlingCallStatusChange = false;
                        _events.OnDeviceStateChange?.Invoke(this, callElement);
                        break;
                }
            }
        }
       
        /// <summary>
        /// Handles changes in the call queue and updates the queued calls collection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="callQueueElement">The call leg element containing the updated queue information.</param>
       
        private void HandleCallQueueChange(object sender, WampClient.wamp_call_leg_element callQueueElement)
        {
            try
            {
                _events.OnCallLogEntryRequested?.Invoke(this, callQueueElement);

                switch (callQueueElement.state)
                {
                    case "init":
                        break;
                    case "ringing":
                    case "waiting":
                        if (_collections.CallQueue.Find(x => x.from_dirno == callQueueElement.from_dirno) == null)
                        {
                            Task.Run(async () => await AddToQueuedCalls(CallLegElement.NewCallLegElementFromSdkElement(callQueueElement)));
                        }
                        break;
                    case "ended":
                    case "in_call":
                        switch (callQueueElement.reason)
                        {
                            case "abandoned": // Could be used for storing missed calls ???
                                break;
                            case "accept":
                            default:
                                Task.Run(async () => await RemoveFromQueuedCalls(callQueueElement));
                                break;
                        }
                        break;
                }
            }
            catch (Exception exe)
            {
                _events.OnExceptionThrown?.Invoke(this, exe);
            }
        }
       
        /// <summary>
        /// Synchronizes the queues and active calls with the system.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
       
        private void HandleQueuesAndCallsSync(object sender, EventArgs e)
        {
            Task.Run(async () => await GetAllCalsAndQueues());
        }
      
        /// <summary>
        /// Updates the operator directory number when a change occurs.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="dirNo">The new operator directory number.</param>
       
        private void HandleOperatorDirectoryNumberChange(object sender, string dirNo)
        {
            _configuration.OperatorDirNo = dirNo;
            _operatorDirno = _configuration.OperatorDirNo;
        }

        #endregion

        #region Call Management Methods

        /// <summary>
        /// Posts a new call and handles call initiation.
        /// </summary>
        /// <param name="from_dir">The directory number of the calling device.</param>
        /// <param name="to_dir">The directory number of the receiving device.</param>
        /// <param name="action">The action to perform (e.g., "answer", "cancel").</param>
        /// <param name="hangUpCurrent">Indicates whether the current active call should be hung up before making a new call.</param>
       
        public async Task PostCall(string from_dir, string to_dir, string action, bool hangUpCurrent = true)
        {
            await Task.Run(() =>
            {
                lock (_postCallLock)
                {
                    try
                    {
                        if (_collections.ActiveCalls.Count > 0)
                        {
                            if (hangUpCurrent)
                            {
                                Task.Run(async () => await DeleteCall(_collections.ActiveCalls[0].dirno));
                            }
                        }
                        _wamp.PostCalls(from_dir, to_dir, action);
                    }
                    catch (Exception exe)
                    {
                        _events.OnExceptionThrown?.Invoke(this, exe);
                    }
                }
            });
        }

        /// <summary>
        /// Adds a device to the active calls list and ensure no duplicate entries are added to the list
        /// </summary>
        /// <param name="device">The device to add.</param>
       
        public async Task AddToActiveCalls(Device device)
        {
            await Task.Run(() =>
            {
                lock (_addToActiveCallsLock)
                {
                    if (device != null)
                    {
                        if (_collections.ActiveCalls.FirstOrDefault(x => x.dirno == device.dirno) == null)
                        {
                            _collections.ActiveCalls.Add(device);
                            this.ActiveDevice = device;
                            _events.OnCallQueueListValueChange?.Invoke(this, EventArgs.Empty);
                            _events.OnActiveCallListValueChange?.Invoke(this, new EventArgs());
                            _events.CallHandlerPopupRequested?.Invoke(this, true);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Removes a device from the active calls list.
        /// </summary>
        /// <param name="device">The device to remove.</param>
       
        public async void RemoveFromActiveCall(Device device)
        {
            await Task.Run(() =>
            {
                lock (_removeFromActiveCallLock)
                {
                    if (_collections.ActiveCalls.Count > 0)
                    {
                        try
                        {
                            _collections.ActiveCalls.Remove(device);
                            this.ActiveDevice = null;
                            _events.OnCallQueueListValueChange?.Invoke(this, EventArgs.Empty);
                            _events.OnActiveCallListValueChange?.Invoke(this, EventArgs.Empty);
                            _events.CallHandlerPopupRequested?.Invoke(this, false);
                        }
                        catch (Exception ex)
                        {
                            _events.OnExceptionThrown?.Invoke(this, ex);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Answers a queued call.
        /// </summary>
        /// <param name="queuedDevice">The queued call to answer.</param>
       
        public async Task AnswerQueuedCall(CallLegElement queuedDevice)
        {
            await Task.Run(() =>
            {
                lock (_answerQueuedCallLock)
                {
                    if (queuedDevice != null)
                    {
                        if (queuedDevice.from_dirno != null)
                        {
                            Task.Run(async () => await PostCall(queuedDevice.from_dirno, _configuration.OperatorDirNo, "answer"));
                            this.ActiveDevice = _collections.RegisteredDevices.FirstOrDefault(x => x.dirno == queuedDevice.from_dirno);
                        }
                    }
                }
            });
        }

       
        /// <summary>
        /// Adds a queued call to the call queue list and ensure no duplicate entries are added to the list
        /// </summary>
        /// <param name="callLeg">The queued call element to add.</param>
        
        public async Task AddToQueuedCalls(CallLegElement callLeg)
        {
            await Task.Run(() =>
            {
                lock (_addToQueuedCallsLock)
                {
                    try
                    {
                        if (callLeg != null)
                        {
                            if (_collections.CallQueue.Where(x => x.from_dirno == callLeg.from_dirno).FirstOrDefault() == null)
                            {
                                _collections.CallQueue.Add(callLeg);
                                this.ActiveDevice = _collections.RegisteredDevices.FirstOrDefault(x => (x.dirno == callLeg.from_dirno));
                                _events.OnCallQueueListValueChange?.Invoke(this, new EventArgs());
                                _events.CallHandlerPopupRequested?.Invoke(this, true);
                            }
                        }
                    }
                    catch (Exception exe)
                    {
                        _events.OnExceptionThrown?.Invoke(this, exe);
                    }
                }
            });
        }
       
        /// <summary>
        /// Removes a queued call from the call queue list.
        /// </summary>
        /// <param name="callLeg">The queued call element to remove.</param>
        
        public async Task RemoveFromQueuedCalls(wamp_call_leg_element callLeg)
        {
            await Task.Run(() =>
            {
                lock (_removeFromQueuedCallsLock)
                {
                    try
                    {
                        Device device = _collections.RegisteredDevices.Where(x => x.dirno == callLeg.from_dirno || x.dirno == callLeg.to_dirno).FirstOrDefault();
                        if (device != null)
                        {
                            CallLegElement callLegElement = _collections.CallQueue.FirstOrDefault(cq => cq.from_dirno == device.dirno || cq.to_dirno == device.dirno);
                            if (callLegElement != null)
                            {
                                _collections.CallQueue.Remove(callLegElement);
                                this.ActiveDevice = _collections.ActiveCalls.FirstOrDefault();
                                _events.OnCallQueueListValueChange?.Invoke(this, EventArgs.Empty);
                                _events.CallHandlerPopupRequested?.Invoke(this, false);
                            }
                        }
                    }
                    catch (Exception exe)
                    {
                        _events.OnExceptionThrown?.Invoke(this, exe);
                    }
                }
            });
        }
       
        /// <summary>
        /// Removes a queued call from the call queue list based on a device.
        /// </summary>
        /// <param name="callDevice">The device associated with the queued call.</param>
        
        public async Task RemoveFromQueuedCalls(Device callDevice)
        {
            await Task.Run(() =>
            {
                lock (_removeFromQueuedCallsLock)
                {
                    try
                    {
                        if (callDevice != null)
                        {
                            CallLegElement callLegElement = _collections.CallQueue.FirstOrDefault(cq => cq.from_dirno == callDevice.dirno || cq.to_dirno == callDevice.dirno);
                            if (callLegElement != null)
                            {
                                _collections.CallQueue.Remove(callLegElement);
                                this.ActiveDevice = _collections.ActiveCalls.FirstOrDefault();
                                _events.OnCallQueueListValueChange?.Invoke(this, EventArgs.Empty);
                                _events.CallHandlerPopupRequested?.Invoke(this, false);
                            }
                        }
                    }
                    catch (Exception exe)
                    {
                        _events.OnExceptionThrown?.Invoke(this, exe);
                    }
                }
            });
        }

        #endregion

        #region Call Synchronization Methods

        /// <summary>
        /// Retrieves and synchronizes all active and queued calls.
        /// </summary>

        private async Task GetAllCalsAndQueues()
        {
            await Task.Run(() =>
            {
                lock (_getAllCalsAndQueuesLock)
                {
                    try
                    {
                        //Convert the lists from sdk 
                        List<CallElement> calls = new List<CallElement>();
                        foreach (var call_element in _wamp.requestCallList("", "", ""))
                        {
                            calls.Add(new CallElement(call_element));
                        }
                        List<CallLegElement> queues = new List<CallLegElement>();
                        foreach (var call_leg_element in _wamp.requestCallLegs("", "", "", "", "", "", ""))
                        {
                            queues.Add(new CallLegElement(call_leg_element));
                        }


                        foreach (CallElement call in calls)
                        {
                            if (call.CallState == CallState.in_call)
                            {
                                if (call.ToDirnoCurrent == _configuration.OperatorDirNo || call.ToDirno == _configuration.OperatorDirNo)
                                    Task.Run(async () => await AddToActiveCalls(_collections.RegisteredDevices.FirstOrDefault(rd => rd.dirno == call.FromDirno)));
                                else
                                    Task.Run(async () => await AddToActiveCalls(_collections.RegisteredDevices.FirstOrDefault(rd => rd.dirno == call.ToDirnoCurrent)));
                            }
                            try
                            {
                                if (_collections.RegisteredDevices.Where(x => x.dirno == call.FromDirno).FirstOrDefault() != null)
                                {
                                    _collections.RegisteredDevices.Where(x => x.dirno == call.FromDirno).FirstOrDefault().CallState = call.CallState; ;
                                }
                                if (_collections.RegisteredDevices.Where(x => x.dirno == call.ToDirnoCurrent).FirstOrDefault() != null)
                                {
                                    _collections.RegisteredDevices.Where(x => x.dirno == call.ToDirnoCurrent).FirstOrDefault().CallState = call.CallState;
                                }
                                _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);
                            }
                            catch (Exception exe)
                            {
                                _events.OnExceptionThrown?.Invoke(this, exe);
                            }
                        }

                        foreach (var call in queues)
                        {
                            if (call.state == CallState.ringing || call.state == CallState.queued)
                            {
                                Task.Run(async () => await AddToQueuedCalls(call));
                            }
                            try
                            {
                                _collections.RegisteredDevices.Where(x => x.dirno == call.from_dirno).FirstOrDefault().CallState = call.state;
                                _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);
                            }
                            catch (Exception exe)
                            {
                                _events.OnExceptionThrown?.Invoke(this, exe);
                            }
                        }
                        _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception exe)
                    {
                        _events.OnExceptionThrown?.Invoke(this, exe);
                    }
                }
            });
        }

        /// <summary>
        /// Retrieves a list of all active calls.
        /// </summary>
        /// <param name="dirno">Optional directory number filter.</param>
        /// <param name="callid">Optional call ID filter.</param>
        /// <param name="state">Optional call state filter.</param>
        /// <returns>A list of active calls.</returns>
        
        public async Task<List<CallElement>> GetAllCals(string dirno = null, string callid = null, string state = null)
        {
            List<CallElement> calls = new List<CallElement>();
            await Task.Run(() =>
            {
                lock (_getAllCalsAndQueuesLock)
                {
                    try
                    {
                        List<Wamp.Client.WampClient.wamp_call_element> wampCalls = _wamp.requestCallList(dirno ?? "", callid ?? "", state ?? "");

                        foreach (wamp_call_element call in wampCalls)
                        {
                            calls.Add(CallElement.NewCallElementFromSdkCallElement(call));
                        }
                    }
                    catch (Exception exe)
                    {
                        _events.OnExceptionThrown?.Invoke(this, exe);
                    }
                }
            });
            return calls;
        }
       
        /// <summary>
        /// Retrieves a list of all queued calls.
        /// </summary>
        /// <param name="dirno">Optional directory number filter.</param>
        /// <param name="fromDirno">Optional originating directory number filter.</param>
        /// <param name="callid">Optional call ID filter.</param>
        /// <returns>A list of queued calls.</returns>
        
        public List<CallLegElement> GetAllQueues(string dirno = "", string fromDirno = "", string callid = "")
        {
            List<CallLegElement> queues = new List<CallLegElement>();
            foreach (var call_leg_element in _wamp.requestCallLegs(fromDirno, fromDirno, dirno, "", callid, "", ""))
            {
                queues.Add(new CallLegElement(call_leg_element));
            }
            return queues;
        }

        #endregion

        #region Call Deletion Methods

        /// <summary>
        /// Deletes all active and queued calls.
        /// </summary>
        
        public async Task DeleteAllCall()
        {

            List<CallElement> allCalls = await GetAllCals();
            lock (_deleteAllCallLock)
            {
                foreach (var call in allCalls)
                {
                    _wamp.DeleteCallId(call.CallId.ToString());
                }
                _collections.ActiveCalls.Clear();
                _collections.CallQueue.Clear();
                _events.OnActiveCallListValueChange?.Invoke(this, EventArgs.Empty);
                _events.OnCallQueueListValueChange?.Invoke(this, EventArgs.Empty);
                _events.OnActiveVideoFeedChange?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Deletes a specific call based on the directory number.
        /// </summary>
        /// <param name="dirno">The directory number of the call to delete.</param>
        
        public async Task DeleteCall(string dirno)
        {
            await Task.Run(() =>
            {
                lock (_deleteCallLock)
                {
                    try
                    {
                        List<wamp_call_element> activeCalls = _wamp.requestCallList(dirno, "", "");
                        List<wamp_call_leg_element> queuedCalls = _wamp.requestCallLegs("", "", "", "", "", "", "");

                        if (activeCalls != null)
                        {
                            wamp_call_element activeCall = FindMatchingCall(activeCalls, dirno);
                            if (activeCall != null)
                            {
                                Task.Run(() => _wamp.DeleteCallId(activeCall.call_id));
                                _events.OnCallLogEntryRequested?.Invoke(this, activeCall);
                                _events.OnActiveVideoFeedChange?.Invoke(this, EventArgs.Empty);
                            }
                        }

                        if (queuedCalls != null)
                        {
                            wamp_call_leg_element queuedCall = FindMatchingQueuedCall(queuedCalls, dirno);
                            if (queuedCall != null)
                            {
                                Task.Run(() => _wamp.DeleteCallId(queuedCall.call_id));
                                _events.OnCallLogEntryRequested?.Invoke(this, queuedCall);
                                _events.OnActiveVideoFeedChange?.Invoke(this, EventArgs.Empty);
                            }
                        }
                    }
                    catch (Exception exe)
                    {
                        _events.OnExceptionThrown?.Invoke(this, exe);
                        _events.OnActiveVideoFeedChange?.Invoke(this, EventArgs.Empty);
                    }
                }
            });
        }

        /// <summary>
        /// Deletes a specific call based on the call ID.
        /// </summary>
        /// <param name="callId">The unique identifier of the call to delete.</param>
        
        public async Task DeleteCall(int callId)
        {
            await Task.Run(() =>
            {
                lock (_deleteCallLock)
                {
                    try
                    {
                        List<CallElement> calls = GetAllCals(null, callId.ToString()).Result;
                        if (calls != null)
                        {
                            if (calls.Count > 0)
                            {
                                _events.OnCallLogEntryRequested?.Invoke(this, calls.First());
                            }
                        }
                        _wamp.DeleteCallId(callId.ToString());
                        _events.OnActiveVideoFeedChange?.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception exe)
                    {
                        _events.OnExceptionThrown?.Invoke(this, exe);
                        _events.OnActiveVideoFeedChange?.Invoke(this, EventArgs.Empty);
                    }
                }
            });
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Finds a matching call from the provided call list based on directory number.
        /// </summary>
        /// <param name="callList">The list of calls to search through.</param>
        /// <param name="dirno">The directory number to match.</param>
        /// <returns>The matching call element, if found; otherwise, null.</returns>
        
        private wamp_call_element FindMatchingCall(List<wamp_call_element> callList, string dirno)
        {
            for (int i = 0; i < callList.Count; i++)
            {
                if (callList[i].from_dirno == dirno || callList[i].to_dirno == dirno || callList[i].to_dirno_current == dirno)
                {
                    return callList[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a matching queued call from the provided queue list based on directory number.
        /// </summary>
        /// <param name="queuedCalls">The list of queued calls to search through.</param>
        /// <param name="dirno">The directory number to match.</param>
        /// <returns>The matching queued call element, if found; otherwise, null.</returns>
        
        private wamp_call_leg_element FindMatchingQueuedCall(List<wamp_call_leg_element> queuedCalls, string dirno)
        {
            for (int i = 0; i < queuedCalls.Count; i++)
            {
                if (queuedCalls[i].from_dirno == dirno || queuedCalls[i].to_dirno == dirno)
                {
                    return queuedCalls[i];
                }
            }
            return null;
        }

        #endregion

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

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_wamp != null)
                {
                    _wamp.OnWampCallStatusEvent -= HandleCallStatusChangedEvent;
                    _wamp.OnWampCallLegStatusEvent -= HandleCallQueueChange;
                }

                if (_events != null)
                {
                    _events.OnQueuesAndCallsSync -= HandleQueuesAndCallsSync;
                    _events.OnOperatorDirNoChange -= HandleOperatorDirectoryNumberChange;
                }

                // Dispose other managed resources here, if added later
            }

            _disposed = true;
        }

        #endregion

    }
}
