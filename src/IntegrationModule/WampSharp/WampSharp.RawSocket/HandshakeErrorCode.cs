#pragma warning disable CS1591
namespace WampSharp.RawSocket
{
    public enum HandshakeErrorCode
    {
        Illegal = 0,
        SerializerUnsupported = 1,
        MaximumMessageLengthUnacceptable = 2,
        UseOfReservedBits = 3,
        MaximumConnectionCountReached = 4
    }
}