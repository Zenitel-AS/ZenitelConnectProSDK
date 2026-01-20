using System;

namespace ConnectPro.Enums
{
    /// <summary>
    /// Defines various states a device can be in within the system.
    /// </summary>
    public enum DeviceState
    {
        /// <summary>The device is reachable and operational.</summary>
        reachable,

        /// <summary>The device is unreachable or offline.</summary>
        unreachable,

        /// <summary>The device state is unknown.</summary>
        unknown
    }

    /// <summary>
    /// Represents different states a call can be in.
    /// </summary>
    public enum CallState
    {
        /// <summary>The call has been initialized but not yet connected.</summary>
        init,

        /// <summary>The call is being forwarded to another destination.</summary>
        forwarding,

        /// <summary>The call is in a queue waiting to be answered.</summary>
        queued,

        /// <summary>The call is currently ringing.</summary>
        ringing,

        /// <summary>The call is actively in progress.</summary>
        in_call,

        /// <summary>The call has ended.</summary>
        ended,

        /// <summary>The destination is reachable but not in a call.</summary>
        reachable,

        /// <summary>The call has encountered a fault or error.</summary>
        fault
    }

    /// <summary>
    /// Represents reasons for a call state change.
    /// </summary>
    public enum Reason
    {
        /// <summary>An incoming call was received.</summary>
        incoming,

        /// <summary>The call was initiated by an RPC (Remote Procedure Call).</summary>
        rpc,

        /// <summary>The destination was busy.</summary>
        busy,

        /// <summary>The call timed out due to no response.</summary>
        timeout,

        /// <summary>The call is in progress.</summary>
        progress,

        /// <summary>The call was unconditional.</summary>
        unconditional,

        /// <summary>The call originated from a ringing state.</summary>
        from_ring,

        /// <summary>The call was accepted.</summary>
        accept,

        /// <summary>The call was answered by another party.</summary>
        other_answer,

        /// <summary>The call was auto-answered.</summary>
        autoanswer,

        /// <summary>The call was canceled by the caller.</summary>
        cancel,

        /// <summary>The call was rejected.</summary>
        reject,

        /// <summary>The call was successfully connected.</summary>
        success,

        /// <summary>The call failed.</summary>
        failure,

        /// <summary>The call was given priority.</summary>
        priority,

        /// <summary>The server terminated the call.</summary>
        server_hangup,

        /// <summary>The call was ended by the caller.</summary>
        hangup
    }

    /// <summary>
    /// Defines different types of calls within the system.
    /// </summary>
    public enum CallType
    {
        /// <summary>A standard person-to-person call.</summary>
        normal_call,

        /// <summary>A call that is placed in a queue to be answered later.</summary>
        queue_call,

        /// <summary>A group call involving multiple participants.</summary>
        group_call,

        /// <summary>A call that plays a prerecorded message.</summary>
        message_play,

        /// <summary>A call that triggers an activation event.</summary>
        activation_call,

        /// <summary>A call that encountered a fault.</summary>
        fault
    }

    /// <summary>
    /// Represents the role of a participant in a call leg.
    /// </summary>
    public enum LegRole
    {
        /// <summary>The participant initiating the call.</summary>
        caller,

        /// <summary>The participant receiving the call.</summary>
        callee
    }

    /// <summary>
    /// Represents the current logical state of a GPIO (General Purpose Input/Output) line.
    /// </summary>
    public enum GpioState
    {
        /// <summary>
        /// The state of the GPIO is unknown or has not yet been reported by the system.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The GPIO line is inactive (logical low / de-asserted).
        /// </summary>
        Inactive = 1,

        /// <summary>
        /// The GPIO line is active (logical high / asserted).
        /// </summary>
        Active = 2
    }

    /// <summary>
    /// Defines the operational direction of a GPIO (General Purpose Input/Output) line.
    /// </summary>
    public enum GpioDirection
    {
        /// <summary>
        /// General Purpose Input (GPI).
        /// The GPIO line is configured as an input and its state is reported by external hardware.
        /// </summary>
        Gpi,

        /// <summary>
        /// General Purpose Output (GPO).
        /// The GPIO line is configured as an output and its state is controlled by the system.
        /// </summary>
        Gpo
    }

}
