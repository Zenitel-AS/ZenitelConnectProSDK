#pragma warning disable CS1591
using System.Threading.Tasks;

namespace WampSharp.WebSockets
{
    public interface IWampWebSocketWrapperConnection
    {
        IClientWebSocketWrapper ClientWebSocket { get; }
        Task RunAsync();
    }
}