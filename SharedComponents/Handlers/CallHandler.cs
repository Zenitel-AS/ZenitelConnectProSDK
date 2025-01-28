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
    public class CallHandler
    {
        private Collections _collections;
        private Events _events;
        private WampClient _wamp;
        private Configuration _configuration;
        private string _operatorDirno;

        private bool IsHandlingCallStatusChange { get; set; } = false;

        private readonly object _postCallLock = new object();
        private readonly object _deleteAllCallLock = new object();
        private readonly object _deleteCallLock = new object();
        private readonly object _addToActiveCallsLock = new object();
        private readonly object _removeFromActiveCallLock = new object();
        private readonly object _answerQueuedCallLock = new object();
        private readonly object _addToQueuedCallsLock = new object();
        private readonly object _removeFromQueuedCallsLock = new object();
        private readonly object _getAllCalsAndQueuesLock = new object();

        public string ParentIpAddress { get; set; } = "";
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
        public Device ActiveDevice { get; set; }

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

        private void HandleCallStatusChangedEvent(object sender, WampClient.wamp_call_element call_Element)
        {
            CallElement callElement = new CallElement(call_Element);
            _events.OnLogEntryRequested?.Invoke(this, callElement);

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

        private void HandleCallQueueChange(object sender, WampClient.wamp_call_leg_element callQueueElement)
        {
            try
            {
                _events.OnLogEntryRequested?.Invoke(this, callQueueElement);

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
        private void HandleQueuesAndCallsSync(object sender, EventArgs e)
        {
            Task.Run(async () => await GetAllCalsAndQueues());
        }
        private void HandleOperatorDirectoryNumberChange(object sender, string dirNo)
        {
            _configuration.OperatorDirNo = dirNo;
            _operatorDirno = _configuration.OperatorDirNo;
        }

        /// <summary>
        /// Post a new call and handle hanging up & adding to ActiveCalls list
        /// </summary>
        /// <param name="from_dir"></param>
        /// <param name="to_dir"></param>
        /// <param name="action"></param>
        /// <param name="addToActiveCallList"></param>
        /// <param name="hangUpCurrent"></param>
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

        private async Task GetAllCalsAndQueues()
        {
            await Task.Run(() =>
            {
                lock (_getAllCalsAndQueuesLock)
                {
                    try
                    {
                        //COnvert the lists from sdk 
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
        public async Task<List<CallElement>> GetAllCals(string dirno = null, string callid = null, string state = null)
        {
            List<CallElement> calls = new List<CallElement>();
            await Task.Run(() =>
            {
                lock (_getAllCalsAndQueuesLock)
                {
                    try
                    {

                        //List<Wamp.Client.WampClient.wamp_call_element> wampCalls = _wamp.requestCallList("", "", "");

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
        public List<CallLegElement> GetAllQueues(string dirno = "", string fromDirno = "", string toDirno = "", string callid = "")
        {
            List<CallLegElement> queues = new List<CallLegElement>();
            foreach (var call_leg_element in _wamp.requestCallLegs(fromDirno, fromDirno, dirno, "", callid, "", ""))
            {
                queues.Add(new CallLegElement(call_leg_element));
            }
            return queues;
        }

        public async Task DeleteAllCall()
        {

            List<CallElement> allCalls = await GetAllCals();
            lock (_deleteAllCallLock)
            {
                //foreach (var call in _collections.ActiveCalls)
                //    _wamp.DeleteCalls(call.dirno);
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
                                _events.OnLogEntryRequested?.Invoke(this, activeCall);
                                _events.OnActiveVideoFeedChange?.Invoke(this, EventArgs.Empty);
                            }
                        }

                        if (queuedCalls != null)
                        {
                            wamp_call_leg_element queuedCall = FindMatchingQueuedCall(queuedCalls, dirno);
                            if (queuedCall != null)
                            {
                                Task.Run(() => _wamp.DeleteCallId(queuedCall.call_id));
                                _events.OnLogEntryRequested?.Invoke(this, queuedCall);
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
                                _events.OnLogEntryRequested?.Invoke(this, calls.First());
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

        /// <summary>
        /// Ensure no duplicate entries are added to the list
        /// </summary>
        /// <param name="device"></param>
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
        /// Ensure no duplicate entries are added to the list
        /// </summary>
        /// <param name="callLeg"></param>
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
    }
}
