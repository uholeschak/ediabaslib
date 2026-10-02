using System;
using RheingoldPsdzWebApi.Adapter.Contracts;

namespace PsdzClient.Contracts
{
    public interface ILifeCycleDependencyProvider
    {
        string Description { get; }

        string Name { get; }

        event EventHandler<DependencyCountChangedEventArgs> ActiveDependencyCountChanged;
    }
}