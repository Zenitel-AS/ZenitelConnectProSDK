using ConnectPro.Enums;
using System;
using System.Collections.Generic;
using static Wamp.Client.WampClient;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents a call element within the system, storing details about the call type, state, participants, and reason.
    /// </summary>
    [Serializable]
    public class CallElement
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for the call.
        /// </summary>
        public int CallId { get; set; }

        /// <summary>
        /// Gets or sets the type of the call.
        /// </summary>
        public CallType CallType { get; set; }

        /// <summary>
        /// Gets or sets the directory number of the caller.
        /// </summary>
        public string FromDirno { get; set; }

        /// <summary>
        /// Gets or sets the leg ID of the caller.
        /// </summary>
        public string FromLegId { get; set; }

        /// <summary>
        /// Gets or sets the list of groups associated with the call.
        /// </summary>
        public List<string> Groups { get; set; }

        /// <summary>
        /// Gets or sets the priority level of the call.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the position of the call in the queue.
        /// </summary>
        public int QueuePos { get; set; }

        /// <summary>
        /// Gets or sets the reason for the call's current state.
        /// </summary>
        public Reason Reason { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the call started.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the current state of the call.
        /// </summary>
        public CallState CallState { get; set; }

        /// <summary>
        /// Gets or sets the directory number of the intended recipient.
        /// </summary>
        public string ToDirno { get; set; }

        /// <summary>
        /// Gets or sets the current directory number the call is directed to.
        /// </summary>
        public string ToDirnoCurrent { get; set; }

        /// <summary>
        /// Gets or sets the collection of call legs associated with this call.
        /// </summary>
        public List<CallLegElement> CallLegs { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CallElement"/> class from a WAMP call element.
        /// </summary>
        /// <param name="wampCallElement">The WAMP call element to initialize the object from.</param>
        public CallElement(wamp_call_element wampCallElement)
        {
            if (!int.TryParse(wampCallElement.call_id, out int callId))
                throw new ArgumentException("Invalid call ID format", nameof(wampCallElement.call_id));

            if (!int.TryParse(wampCallElement.priority, out int priority))
                throw new ArgumentException("Invalid priority format", nameof(wampCallElement.priority));

            CallId = callId;
            FromDirno = wampCallElement.from_dirno;
            FromLegId = wampCallElement.from_leg_id;
            Priority = priority;
            ToDirno = wampCallElement.to_dirno;
            ToDirnoCurrent = wampCallElement.to_dirno_current;

            CallType = Enum.TryParse(wampCallElement.call_type, ignoreCase: true, out CallType callType)
                ? callType
                : CallType.fault;

            Reason = Enum.TryParse(wampCallElement.reason, ignoreCase: true, out Reason callReason)
                ? callReason
                : Reason.failure;

            CallState = Enum.TryParse(wampCallElement.state, ignoreCase: true, out CallState callState)
                ? callState
                : CallState.fault;

            CallLegs = new List<CallLegElement>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new instance of the <see cref="CallElement"/> class from a WAMP call element.
        /// </summary>
        /// <param name="wampCallElement">The WAMP call element to convert.</param>
        /// <returns>A new instance of <see cref="CallElement"/> with properties populated from the WAMP call element.</returns>
        public static CallElement NewCallElementFromSdkCallElement(wamp_call_element wampCallElement)
        {
            return new CallElement(wampCallElement);
        }

        #endregion
    }
}
