using ConnectPro.Models;
using ConnectPro.Tools;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Wamp.Client.WampClient;
using Wamp.Client;
using Microsoft.Extensions.Logging.Abstractions;

namespace ConnectPro.Handlers
{
    public class CallHandler
    {
        private Collections _collections;
        private Events _events;
        private WampClient _wamp;

        private bool IsHandlingCallStatusChange { get; set; } = false;
        private object _lock = new object();
        private string _operatorDirNo = "";

        public CallHandler(Collections collections, Events events, WampClient wamp, string operatorDirNo)
        {
            _collections = collections;
            _events = events;
            _wamp = wamp;
            _operatorDirNo = operatorDirNo;

            _wamp.OnWampCallStatusEvent += HandleCallStatusChangedEvent;
            _wamp.OnWampCallLegStatusEvent += HandleCallQueueChange;

            _events.OnQueuesAndCallsSync += HandleQueuesAndCallsSync;
            _events.OnOperatorDirNoChange += HandleOperatorDirectoryNumberChange;
        }

        private void HandleCallStatusChangedEvent(object sender, WampClient.wamp_call_element call_Element)
        {
            if (this.IsHandlingCallStatusChange == false)
            {
                this.IsHandlingCallStatusChange = true;
                Device device;

                if (_collections.RegisteredDevices.Count == 0)
                    return;

                if (call_Element.from_dirno == _operatorDirNo)
                    device = _collections.RegisteredDevices.FirstOrDefault(rd => rd.dirno == call_Element.to_dirno);
                else
                    device = _collections.RegisteredDevices.FirstOrDefault(rd => rd.dirno == call_Element.from_dirno);

                if (device == null)
                    return;

                switch (call_Element.state)
                {
                    case "in_call":
                        if (call_Element.to_dirno_current == _operatorDirNo || call_Element.from_dirno == _operatorDirNo)
                            AddToActiveCalls(device);
                        _events.OnDeviceStateChange?.Invoke(this, call_Element);
                        _events.OnActiveVideoFeedChange?.Invoke(this, EventArgs.Empty);
                        _events.OnLogEntryRequested?.Invoke(this, call_Element);
                        this.IsHandlingCallStatusChange = false;
                        break;
                    case "ended":
                        Device activeDevice = _collections.ActiveCalls.Find(ac =>
                                        ac.dirno == call_Element.from_dirno
                                        || ac.dirno == call_Element.to_dirno);

                        if (activeDevice != null)
                        {
                            RemoveFromActiveCall(activeDevice);
                        }
                        _events.OnDeviceStateChange?.Invoke(this, call_Element);
                        _events.OnLogEntryRequested?.Invoke(this, call_Element);
                        this.IsHandlingCallStatusChange = false;

                        break;
                    case "queued":
                    case "ringing":
                        _events.OnDeviceStateChange?.Invoke(this, call_Element);
                        _events.OnActiveVideoFeedChange?.Invoke(this, EventArgs.Empty);
                        this.IsHandlingCallStatusChange = false;
                        break;
                    default:
                        this.IsHandlingCallStatusChange = false;
                        _events.OnDeviceStateChange?.Invoke(this, call_Element);
                        break;
                }
            }
        }
        private void HandleCallQueueChange(object sender, WampClient.wamp_call_leg_element callQueueElement)
        {
            try
            {
                switch (callQueueElement.state)
                {
                    case "init":

                        break;
                    case "ringing":
                    case "waiting":
                        if (_collections.CallQueue.Find(x => x.from_dirno == callQueueElement.from_dirno) == null)
                        {
                            AddToQueuedCalls(CallLegElement.NewCallLegElementFromSdkElement(callQueueElement));
                            _events.OnLogEntryRequested?.Invoke(this, callQueueElement);
                        }
                        break;
                    case "ended":
                    case "in_call":
                        switch (callQueueElement.reason)
                        {
                            case "abandoned": // Could be used for storing missed calls ???
                                _events.OnLogEntryRequested?.Invoke(this, callQueueElement);
                                break;
                            case "accept":
                            default:
                                RemoveFromQueuedCalls(callQueueElement);
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
            GetAllCalsAndQueues();
        }
        private void HandleOperatorDirectoryNumberChange(object sender, string dirNo)
        {
            _operatorDirNo = dirNo;
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
                lock (_lock)
                {
                    try
                    {
                        if (_collections.ActiveCalls.Count > 0)
                        {
                            if (hangUpCurrent)
                            {
                                DeleteCall(_collections.ActiveCalls[0].dirno);
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

        public void DeleteAllCall()
        {
            lock (_lock)
            {
                foreach (var call in _collections.ActiveCalls)
                    _wamp.DeleteCalls(call.dirno);

                _collections.ActiveCalls.Clear();
                _events.OnActiveCallListValueChange?.Invoke(this, EventArgs.Empty);
            }
        }
        public void DeleteCall(string dirno)
        {
            lock (_lock)
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
                        }
                    }

                    if (queuedCalls != null)
                    {
                        wamp_call_leg_element queuedCall = FindMatchingQueuedCall(queuedCalls, dirno);
                        if (queuedCall != null)
                        {
                            Task.Run(() => _wamp.DeleteCallId(queuedCall.call_id));
                        }
                    }
                }
                catch (Exception exe)
                {
                    _events.OnExceptionThrown?.Invoke(this, exe);
                }
            }
        }
        private void GetAllCalsAndQueues()
        {
            try
            {
                List<wamp_call_element> calls = _wamp.requestCallList("", "", "");
                foreach (wamp_call_element call in calls)
                {
                    if (call.state == "in_call")
                    {
                        if (call.to_dirno_current == _operatorDirNo || call.to_dirno == _operatorDirNo)
                            AddToActiveCalls(_collections.RegisteredDevices.FirstOrDefault(rd => rd.dirno == call.from_dirno));
                        else
                            AddToActiveCalls(_collections.RegisteredDevices.FirstOrDefault(rd => rd.dirno == call.to_dirno_current));
                    }
                    try
                    {
                        _collections.RegisteredDevices.Where(x => x.dirno == call.from_dirno).FirstOrDefault().state = call.state;
                        _collections.RegisteredDevices.Where(x => x.dirno == call.to_dirno_current).FirstOrDefault().state = call.state;
                        _events.OnDeviceListChange?.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception exe)
                    {
                        _events.OnExceptionThrown?.Invoke(this, exe);
                    }
                }
                List<wamp_call_leg_element> queues = _wamp.requestCallLegs("", "", "", "", "", "", "");
                foreach (wamp_call_leg_element call in queues)
                {
                    if (call.state == "ringing" || call.state == "queued")
                    {
                        AddToQueuedCalls(CallLegElement.NewCallLegElementFromSdkElement(call));
                    }
                    try
                    {
                        _collections.RegisteredDevices.Where(x => x.dirno == call.from_dirno).FirstOrDefault().state = call.state;
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
        /// <summary>
        /// Ensure no duplicate entries are added to the list
        /// </summary>
        /// <param name="device"></param>
        public void AddToActiveCalls(Device device)
        {
            lock (_lock)
            {
                if (device != null)
                {
                    if (_collections.ActiveCalls.FirstOrDefault(x => x.dirno == device.dirno) == null)
                    {
                        _collections.ActiveCalls.Add(device);
                        _events.OnActiveCallListValueChange?.Invoke(this, new EventArgs());
                        _events.CallHandlerPopupRequested?.Invoke(this, true);
                    }
                }
            }
        }
        public void RemoveFromActiveCall(Device device)
        {
            _collections.ActiveCalls.Remove(device);
            _events.OnActiveCallListValueChange?.Invoke(this, EventArgs.Empty);
            _events.CallHandlerPopupRequested?.Invoke(this, false);
        }
        public void AnswerQueuedCall(CallLegElement queuedDevice)
        {
            if (queuedDevice != null)
            {
                if (queuedDevice.from_dirno != null)
                {
                    Task.Run(async () => await PostCall(queuedDevice.from_dirno, _operatorDirNo, "answer"));
                }
            }
        }
        /// <summary>
        /// Ensure no duplicate entries are added to the list
        /// </summary>
        /// <param name="callLeg"></param>
        public void AddToQueuedCalls(CallLegElement callLeg)
        {
            lock (_lock)
            {
                try
                {
                    if (callLeg != null)
                    {
                        if (_collections.CallQueue.Where(x => x.from_dirno == callLeg.from_dirno).FirstOrDefault() == null)
                        {
                            _collections.CallQueue.Add(callLeg);
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
        }
        public void RemoveFromQueuedCalls(wamp_call_leg_element callLeg)
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
