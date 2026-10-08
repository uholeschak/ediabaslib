using System;

namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public interface ILifeCycleDependencyProvider
    {
        string Description { get; }

        string Name { get; }

        event EventHandler<DependencyCountChangedEventArgs> ActiveDependencyCountChanged;
    }
}