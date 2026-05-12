using ConnectPro.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConnectPro.Tools
{
   
    public class CallStateMapper
    {
        /// <summary>
        /// Determines whether the specified call state should be treated as a busy state.
        /// </summary>
        /// <param name="state">The call state to evaluate.</param>
        /// <returns><see langword="true"/> when the state represents an active or pending call; otherwise, <see langword="false"/>.</returns>
        public static bool IsBusy(CallState state)
        {
            switch (state)
            {
                case CallState.init:
                case CallState.forwarding:
                case CallState.queued:
                case CallState.ringing:
                case CallState.in_call:
                    return true;

                case CallState.reachable:
                case CallState.ended:
                case CallState.fault:
                    return false;

                default:
                    return false;
            }
        }
    }
}
