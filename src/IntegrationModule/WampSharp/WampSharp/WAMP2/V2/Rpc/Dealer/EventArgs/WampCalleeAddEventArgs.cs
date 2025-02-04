#pragma warning disable CS1591
namespace WampSharp.V2.Rpc
{
    public class WampCalleeAddEventArgs : WampCalleeChangeEventArgs
    {
        public WampCalleeAddEventArgs(IWampRpcOperation operation) : base(operation)
        {
        }
    }
}