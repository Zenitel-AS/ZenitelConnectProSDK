#pragma warning disable CS1591
using System;
using System.Runtime.Serialization;
using WampSharp.Core.Message;

namespace WampSharp.V2.Core.Contracts
{
    [DataContract]
    [Serializable]
    [WampDetailsOptions(WampMessageType.v2Challenge)]
    public class ChallengeDetails : WampDetailsOptions
    {
    }
}