using System;

namespace PsdzClient.Psdz
{
    public interface IPsdzShutdownEventProvider
    {
        event EventHandler<EventArgs> ShutdownRequested;
    }
}