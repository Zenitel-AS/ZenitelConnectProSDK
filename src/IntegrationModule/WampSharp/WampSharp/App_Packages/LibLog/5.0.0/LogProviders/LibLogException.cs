#pragma warning disable CS1591
namespace WampSharp.Logging.LogProviders
{
    using System;

    public class LibLogException : Exception
    {
        public LibLogException(string message)
            : base(message)
        {
        }

        public LibLogException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
