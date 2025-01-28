using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectPro.Enums
{
    public enum DeviceState { reachable, unreachable, unknown }
    public enum CallState { init, forwarding, queued, ringing, in_call, ended, reachable, fault };
    public enum Reason { incoming, rpc, busy, timeout, progress, unconditional, from_ring, accept, other_answer, autoanswer, cancel, reject, success, failure, priority, server_hangup, hangup };
    public enum CallType { normal_call, queue_call, group_call, message_play, activation_call, fault };
    public enum LegRole { caller, callee };
}
