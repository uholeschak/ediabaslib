using System;

namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public interface IPsdzShutdownEventProvider
    {
        event EventHandler<EventArgs> ShutdownRequested;
    }
}