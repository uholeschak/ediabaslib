using System;
using System.Collections.Generic;
using PsdzClient.Psdz;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzServiceInternal : IPsdzService, IDisposable
    {
        IEnumerable<ILifeCycleDependencyProvider> LifeCycleDependencyProvider { get; }

        IPsdzShutdownEventProvider ShutdownEventProvider { get; }

        void ResetRootDirectory(object sender, EventArgs e);
    }
}
