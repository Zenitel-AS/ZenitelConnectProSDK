using ConnectPro.Models;
using System;

namespace ConnectPro
{
    public class Events
    {
        public EventHandler<object> OnManualVideoFeedChange { get; set; }
        public EventHandler OnActiveVideoFeedChange { get; set; }
        public EventHandler<string> OnOperatorDirNoChange { get; set; }
        public EventHandler OnDeviceListChange { get; set; }
        public EventHandler<object> OnDeviceStateChange { get; set; }
        public EventHandler OnActiveCallListValueChange { get; set; }
        public EventHandler OnCallQueueListValueChange { get; set; }
        public EventHandler OnOutgoingPrivateCall { get; set; }
        public EventHandler<(string, object)> OnDebugChanged { get; set; }
        public EventHandler<object> OnLogEntryRequested { get; set; }
        public EventHandler<CallLog> OnLogEntryAdded { get; set; }
        public EventHandler OnGroupsListChange { get; set; }
        public EventHandler<object> OnGroupsMsgUpdate { get; set; }
        public EventHandler OnDeviceRetrievalStart { get; set; }
        public EventHandler OnDeviceRetrievalEnd { get; set; }
        public EventHandler OnQueuesAndCallsSync { get; set; }
        public EventHandler<Exception> OnExceptionThrown { get; set; }
        public EventHandler<bool> OnConnectionChanged;
        /// <summary>
        /// Open/Close popup window
        /// </summary>
        public EventHandler<bool> CallHandlerPopupRequested { get; set; }
    }
}
