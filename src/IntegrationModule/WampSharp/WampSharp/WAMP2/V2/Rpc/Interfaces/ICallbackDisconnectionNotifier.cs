#pragma warning disable CS1591
using System;

namespace WampSharp.V2.Rpc
{
    public interface ICallbackDisconnectionNotifier
    {
        event EventHandler Disconnected;
    }
}