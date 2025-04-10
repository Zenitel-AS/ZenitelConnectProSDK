﻿#pragma warning disable CS1591
using WampSharp.Core.Serialization;
using WampSharp.V2.Binding.Parsers;

namespace WampSharp.V2.Binding.Contracts
{
    /// <summary>
    /// A base class that represents WAMP2 wamp.2.cbor binding.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class CborBinding<TMessage> : WampTransportBinding<TMessage, byte[]>,
        IWampBinaryBinding<TMessage>
    {
        protected CborBinding(IWampFormatter<TMessage> formatter, IWampBinaryMessageParser<TMessage> parser)
            : base(formatter, parser, WampSubProtocols.CborSubProtocol)
        {
        }
    }
}
