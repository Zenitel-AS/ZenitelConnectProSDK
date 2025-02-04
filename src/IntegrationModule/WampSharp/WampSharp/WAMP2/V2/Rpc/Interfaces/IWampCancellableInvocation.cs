#pragma warning disable CS1591
using WampSharp.V2.Core.Contracts;

namespace WampSharp.V2.Rpc
{
    public interface IWampCancellableInvocation
    {
        void Cancel(InterruptDetails details);
    }
}