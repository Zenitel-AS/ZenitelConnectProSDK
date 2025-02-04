#if !NETSTANDARD2_1 && !NETCOREAPP3_0_OR_GREATER && !NET5_0_OR_GREATER

namespace System.Runtime.CompilerServices
{
    internal class RuntimeFeature
    {
        public const bool IsDynamicCodeCompiled = true;
        public const bool IsDynamicCodeSupported = true;
    }
}

#endif