using System;
using System.Collections.Generic;

namespace ConnectPro.Models
{
    /// <summary>
    /// Call queue element defined in the schema.
    ///
    /// Note:
    /// - No JSON attributes are used to avoid adding packages (System.Text.Json / Newtonsoft).
    /// - Property names are kept snake_case to match the schema payload keys directly.
    /// </summary>
    [Serializable]
    public sealed class CallQueue
    {
        public CallQueue(Wamp.Client.WampClient.wamp_call_queue_element wampQueue)
        {
            queue_dirno = wampQueue.queue_dirno;

            calls = new List<CallElement>();
            if (wampQueue.calls != null)
            {
                foreach (var wampCall in wampQueue.calls)
                {
                    calls.Add(new CallElement(wampCall));
                }
            }

            operators = new List<string>();
            if (wampQueue.operators != null)
            {
                foreach (var op in wampQueue.operators)
                {
                    operators.Add(op.dirno);
                }
            }
        }
        /// <summary>
        /// The directory number of the call queue.
        /// Example: "701"
        /// </summary>
        public string queue_dirno { get; set; }

        /// <summary>
        /// Calls queued in the call queue. Listed in order (first in queue first).
        /// Schema type: call_rich[]
        /// Reuse existing CallElement to avoid new/duplicate model types.
        /// </summary>
        public List<CallElement> calls { get; set; }

        /// <summary>
        /// Operators of the call queue.
        /// Schema shape is operators[] = { dirno: "112" }.
        ///
        /// Without adding a new Operator DTO (to avoid collisions with your SDK Operator model),
        /// we store just the operator directory numbers.
        /// </summary>
        public List<string> operators { get; set; }

        public CallQueue()
        {
            queue_dirno = string.Empty;
            calls = new List<CallElement>();
            operators = new List<string>();
        }
    }
}
