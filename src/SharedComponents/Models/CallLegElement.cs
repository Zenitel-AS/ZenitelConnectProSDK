using ConnectPro.Enums;
using ConnectPro.Interfaces;
using System;
using System.Collections.Generic;
using static Wamp.Client.WampClient;

namespace ConnectPro.Models
{
    /// <summary>
    /// Represents a call leg element, storing details about a specific leg of a call including call type, state, priority, and associated cameras.
    /// </summary>
    [Serializable]
    public class CallLegElement
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique call identifier.
        /// </summary>
        public string call_id { get; set; }

        /// <summary>
        /// Gets or sets the type of the call.
        /// </summary>
        public CallType? call_type { get; set; }

        /// <summary>
        /// Gets or sets the communication channel used for the call leg.
        /// </summary>
        public string channel { get; set; }

        /// <summary>
        /// Gets or sets the directory number associated with this call leg.
        /// </summary>
        public string dirno { get; set; }

        /// <summary>
        /// Gets or sets the name associated with this call leg.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the directory number of the calling device.
        /// </summary>
        public string from_dirno { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the call leg.
        /// </summary>
        public string leg_id { get; set; }

        /// <summary>
        /// Gets or sets the role of this call leg (caller or callee).
        /// </summary>
        public string leg_role { get; set; }

        /// <summary>
        /// Gets or sets the priority of the call leg.
        /// </summary>
        public string priority { get; set; }

        /// <summary>
        /// Gets or sets the reason for the call's current state.
        /// </summary>
        public Reason? reason { get; set; }

        /// <summary>
        /// Gets or sets the list of cameras associated with this call leg.
        /// </summary>
        public List<ICamera> Cameras { get; set; }

        /// <summary>
        /// Gets or sets the current state of the call leg (e.g., join/leave).
        /// </summary>
        public CallState? state { get; set; }

        /// <summary>
        /// Gets or sets the directory number of the recipient.
        /// </summary>
        public string to_dirno { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CallLegElement"/> class.
        /// </summary>
        public CallLegElement()
        {
            Cameras = new List<ICamera>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallLegElement"/> class from a WAMP call leg element.
        /// </summary>
        /// <param name="sdk_element">The WAMP call leg element to initialize the object from.</param>
        public CallLegElement(wamp_call_leg_element sdk_element)
        {
            name = "";
            call_id = sdk_element.call_id;
            call_type = ParseEnum<CallType>(sdk_element.call_type);
            channel = sdk_element.channel;
            dirno = sdk_element.dirno;
            from_dirno = sdk_element.from_dirno;
            leg_id = sdk_element.leg_id;
            leg_role = sdk_element.leg_role; 
            priority = sdk_element.priority;
            reason = ParseEnum<Reason>(sdk_element.reason);
            state = ParseEnum<CallState>(sdk_element.state);
            to_dirno = sdk_element.to_dirno;
            Cameras = new List<ICamera>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new instance of the <see cref="CallLegElement"/> class from a WAMP call leg element.
        /// </summary>
        /// <param name="sdk_element">The WAMP call leg element to convert.</param>
        /// <returns>A new instance of <see cref="CallLegElement"/> with properties populated from the WAMP call leg element.</returns>
        public static CallLegElement NewCallLegElementFromSdkElement(wamp_call_leg_element sdk_element)
        {
            return new CallLegElement(sdk_element);
        }

        /// <summary>
        /// Parses an enum value from a string, returning null if parsing fails.
        /// </summary>
        /// <typeparam name="T">The enum type to parse.</typeparam>
        /// <param name="value">The string value to parse.</param>
        /// <returns>The parsed enum value, or null if parsing fails.</returns>
        private static T? ParseEnum<T>(string value) where T : struct
        {
            return Enum.TryParse(value, ignoreCase: true, out T result) ? result : (T?)null;
        }

        #endregion
    }
}
