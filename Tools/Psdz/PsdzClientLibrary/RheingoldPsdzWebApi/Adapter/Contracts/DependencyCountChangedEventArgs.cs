using System;

namespace RheingoldPsdzWebApi.Adapter.Contracts
{
    public class DependencyCountChangedEventArgs : EventArgs
    {
        public int ItemCount { get; private set; }

        public DependencyCountChangedEventArgs(int itemCount)
        {
            ItemCount = itemCount;
        }
    }
}