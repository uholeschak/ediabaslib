using System;
using System.Collections.Generic;
using RheingoldPsdzWebApi.Adapter.Contracts.Services;

namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public interface IPsdzServiceInternal : IPsdzService, IDisposable
    {
        IEnumerable<ILifeCycleDependencyProvider> LifeCycleDependencyProvider { get; }

        IPsdzShutdownEventProvider ShutdownEventProvider { get; }

        void ResetRootDirectory(object sender, EventArgs e);
    }
}
