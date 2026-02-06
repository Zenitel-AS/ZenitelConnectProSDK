using System;
using ZenitelConnectProOperator.Core.Abstractions;

namespace ZenitelConnectProOperator.Core.Services;

public sealed class AudioService : IAudioService
{
    public void PlayConnected()
    {
        try { Console.Beep(880, 120); }
        catch { /* never crash due to audio */ }
    }
}
