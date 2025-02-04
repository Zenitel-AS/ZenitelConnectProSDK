#pragma warning disable CS1591
namespace WampSharp.V2.Rpc
{
    public interface IRemoteWampCalleeOperation : IWampRpcOperation
    {
        long SessionId { get; }         
    }
}