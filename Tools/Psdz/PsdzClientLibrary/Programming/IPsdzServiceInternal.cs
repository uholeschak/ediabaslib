using System;
using System.Collections.Generic;
using PsdzClient.Contracts;
using PsdzClient.Psdz;

namespace BMW.Rheingold.Psdz
{
    public interface IPsdzServiceInternal : IPsdzService, IDisposable
    {
        IEnumerable<ILifeCycleDependencyProvider> LifeCycleDependencyProvider { get; }

        IPsdzShutdownEventProvider ShutdownEventProvider { get; }

        void ResetRootDirectory(object sender, EventArgs e);
    }
}
